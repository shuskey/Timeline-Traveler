using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.DataObjects;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;
using UnityEngine;

namespace Assets.Scripts.ServiceProviders
{
    /// <summary>
    /// Builder class to construct a FamilyDAG from existing data providers
    /// This bridges the gap between your current data loading and the new DAG structure
    /// </summary>
    public class FamilyDAGBuilder
    {
        private readonly IFamilyHistoryDataProvider _dataProvider;
        private readonly FamilyDAG _familyDAG;
        private readonly HashSet<int> _processedPeople = new HashSet<int>();
        private readonly HashSet<(int, int, PersonRelationshipType)> _processedRelationships = new HashSet<(int, int, PersonRelationshipType)>();

        public FamilyDAGBuilder(IFamilyHistoryDataProvider dataProvider)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _familyDAG = new FamilyDAG();
        }

        /// <summary>
        /// Build a complete family DAG starting from a root person
        /// This replaces the current generation-based loading approach
        /// Enhanced to support all "Close Family" relationships from FamilyDefinitions.md
        /// </summary>
        public FamilyDAG BuildDAGForPerson(int rootPersonId, int ancestryDepth = 5, int descendancyDepth = 5)
        {
            // Load root person first
            var rootPerson = LoadPersonIntoDAG(rootPersonId);
            
            // Load spouses for root person (matches current system behavior)
            if (rootPerson != null)
            {
                LoadSpousesForPerson(rootPersonId, rootPerson);
            }
            
            // Load ancestry (parents, grandparents, etc.)
            LoadAncestryRecursive(rootPersonId, ancestryDepth);
            
            // Load descendants (children, grandchildren, etc.)
            LoadDescendantsRecursive(rootPersonId, descendancyDepth);
            
            // Load additional people needed for "Close Family" relationships
            LoadCloseFamilyMembers(rootPersonId);
            
            // Build sibling relationships
            BuildSiblingRelationships();
            
            // Build extended relationships (aunts, uncles, cousins, etc.)
            BuildExtendedRelationships();
            
            return _familyDAG;
        }

        /// <summary>
        /// Load a single person into the DAG if not already loaded
        /// </summary>
        private Person LoadPersonIntoDAG(int personId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            if (_processedPeople.Contains(personId))
            {
                return _familyDAG.People[personId];
            }

            var personList = _dataProvider.GetPerson(personId, generation, xOffset, spouseNumber);
            if (personList.Count == 0)
            {
                Debug.LogWarning($"Person {personId} not found in data provider");
                return null;
            }

            var person = personList[0];
            _familyDAG.AddPerson(person);
            _processedPeople.Add(personId);
            
            return person;
        }

        /// <summary>
        /// Recursively load ancestry and build parent-child relationships
        /// Also includes spouse loading to match current system behavior
        /// </summary>
        private void LoadAncestryRecursive(int personId, int remainingDepth)
        {
            if (remainingDepth <= 0) return;

            // Load spouses for this person (matches current system behavior)
            var person = _familyDAG.People.ContainsKey(personId) ? _familyDAG.People[personId] : null;
            if (person != null)
            {
                LoadSpousesForPerson(personId, person);
            }

            var parentageList = _dataProvider.GetParents(personId);
            foreach (var parentage in parentageList)
            {
                // Load and link father
                if (parentage.fatherId != 0)
                {
                    var father = LoadPersonIntoDAG(parentage.fatherId);
                    if (father != null)
                    {
                        AddRelationshipSafely(parentage.fatherId, personId, PersonRelationshipType.Father);
                        
                        // Always load spouses for father even if we don't recurse further
                        LoadSpousesForPerson(parentage.fatherId, father);

                        // Continue ancestry from father
                        LoadAncestryRecursive(parentage.fatherId, remainingDepth - 1);
                    }
                }

                // Load and link mother
                if (parentage.motherId != 0)
                {
                    var mother = LoadPersonIntoDAG(parentage.motherId);
                    if (mother != null)
                    {
                        AddRelationshipSafely(parentage.motherId, personId, PersonRelationshipType.Mother);
                        
                        // Always load spouses for mother even if we don't recurse further
                        LoadSpousesForPerson(parentage.motherId, mother);

                        // Continue ancestry from mother
                        LoadAncestryRecursive(parentage.motherId, remainingDepth - 1);
                    }
                }

                // Load marriage relationship between parents
                if (parentage.fatherId != 0 && parentage.motherId != 0)
                {
                    var marriages = _dataProvider.GetMarriages(parentage.fatherId, true);
                    var relevantMarriage = marriages.FirstOrDefault(m => m.wifeId == parentage.motherId);
                    if (relevantMarriage != null)
                    {
                        AddRelationshipSafely(parentage.fatherId, parentage.motherId, PersonRelationshipType.Spouse, relevantMarriage.marriageYear);
                        AddRelationshipSafely(parentage.motherId, parentage.fatherId, PersonRelationshipType.Spouse, relevantMarriage.marriageYear);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively load descendants and build parent-child relationships
        /// This matches the current Tribe system's approach including spouse loading at each generation
        /// </summary>
        private void LoadDescendantsRecursive(int personId, int remainingDepth)
        {
            if (remainingDepth <= 0) return;

            var person = _familyDAG.People.ContainsKey(personId) ? _familyDAG.People[personId] : LoadPersonIntoDAG(personId);
            if (person == null) return;

            // Load spouses for this person (matches AddSpousesAndFixUpDates behavior)
            LoadSpousesForPerson(personId, person);

            bool isHusbandQuery = person.gender == PersonGenderType.Male;
            var marriages = _dataProvider.GetMarriages(personId, isHusbandQuery);

            foreach (var marriage in marriages)
            {
                // Load children of this marriage
                var children = _dataProvider.GetChildren(marriage.familyId);
                foreach (var childInfo in children)
                {
                    var child = LoadPersonIntoDAG(childInfo.childId);
                    if (child != null)
                    {
                        var spouseId = isHusbandQuery ? marriage.wifeId : marriage.husbandId;
                        var spouse = _familyDAG.People.ContainsKey(spouseId) ? _familyDAG.People[spouseId] : null;
                        
                        // Add parent-child relationships
                        AddRelationshipSafely(personId, childInfo.childId, PersonRelationshipType.Child);
                        
                        if (spouse != null)
                        {
                            AddRelationshipSafely(spouseId, childInfo.childId, PersonRelationshipType.Child);
                        }

                        // Always load spouses for children even if we don't recurse further
                        // This matches current system behavior where spouses are loaded at every generation
                        LoadSpousesForPerson(childInfo.childId, child);

                        // Continue descendants from child
                        LoadDescendantsRecursive(childInfo.childId, remainingDepth - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Load spouses for a person - matches the current Tribe system's AddSpousesAndFixUpDates method
        /// </summary>
        private void LoadSpousesForPerson(int personId, Person person)
        {
            bool isHusbandQuery = person.gender == PersonGenderType.Male;
            var marriages = _dataProvider.GetMarriages(personId, isHusbandQuery);

            foreach (var marriage in marriages)
            {
                var spouseId = isHusbandQuery ? marriage.wifeId : marriage.husbandId;
                
                // Load spouse if not already loaded
                var spouse = LoadPersonIntoDAG(spouseId);
                if (spouse != null)
                {
                    // Add marriage relationships
                    AddRelationshipSafely(personId, spouseId, PersonRelationshipType.Spouse, marriage.marriageYear);
                    AddRelationshipSafely(spouseId, personId, PersonRelationshipType.Spouse, marriage.marriageYear);
                    
                    // Apply date fixing logic (matches current system)
                    person.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, spouse);
                    spouse.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, person);
                }
                else
                {
                    Debug.LogWarning($"  -> Could not load spouse with ID: {spouseId}");
                }
            }
        }

        /// <summary>
        /// Load additional people needed for "Close Family" relationships as defined in FamilyDefinitions.md
        /// This ensures all siblings, aunts/uncles, cousins, nieces/nephews are available in the DAG
        /// </summary>
        private void LoadCloseFamilyMembers(int rootPersonId)
        {
            var rootPerson = _familyDAG.People.ContainsKey(rootPersonId) ? _familyDAG.People[rootPersonId] : null;
            if (rootPerson == null) return;

            // 1. Load siblings of the focus person (and their spouses and children)
            LoadSiblingsAndFamily(rootPersonId);

            // 2. Load aunts and uncles (siblings of parents) and their families
            LoadAuntsUnclesAndFamily(rootPersonId);

            // 3. Load nieces and nephews (children of siblings)
            LoadNiecesNephews(rootPersonId);

            // 4. Load cousins (children of aunts and uncles)
            LoadCousins(rootPersonId);
        }

        /// <summary>
        /// Load siblings of a person and their families
        /// </summary>
        private void LoadSiblingsAndFamily(int personId)
        {
            var parentageList = _dataProvider.GetParents(personId);
            
            foreach (var parentage in parentageList)
            {
                // Load siblings through father
                if (parentage.fatherId != 0)
                {
                    LoadSiblingsFromParent(parentage.fatherId, personId);
                }
                
                // Load siblings through mother
                if (parentage.motherId != 0)
                {
                    LoadSiblingsFromParent(parentage.motherId, personId);
                }
            }
        }

        /// <summary>
        /// Load siblings of a specific parent (excluding the focus person)
        /// </summary>
        private int LoadSiblingsFromParent(int parentId, int excludePersonId)
        {
            var parent = _familyDAG.People.ContainsKey(parentId) ? _familyDAG.People[parentId] : LoadPersonIntoDAG(parentId);
            if (parent == null) return 0;

            int siblingsLoaded = 0;
            
            // Get all children of this parent
            var marriages = _dataProvider.GetMarriages(parentId, parent.gender == PersonGenderType.Male);
            
            foreach (var marriage in marriages)
            {
                var children = _dataProvider.GetChildren(marriage.familyId);
                
                foreach (var childInfo in children)
                {
                    if (childInfo.childId != excludePersonId) // Don't reload the focus person
                    {
                        var sibling = LoadPersonIntoDAG(childInfo.childId);
                        if (sibling != null)
                        {
                            siblingsLoaded++;
                            
                            // CRITICAL FIX: Create parent-child relationships in the DAG
                            // This ensures GetSiblingsFromDAG can find the sibling through parent-child links
                            if (parent.gender == PersonGenderType.Male)
                            {
                                AddRelationshipSafely(parentId, childInfo.childId, PersonRelationshipType.Father);
                            }
                            else
                            {
                                AddRelationshipSafely(parentId, childInfo.childId, PersonRelationshipType.Mother);
                            }
                            
                            // Load spouse(s) of this sibling
                            LoadSpousesForPerson(childInfo.childId, sibling);
                            
                            // Load children of this sibling (nieces/nephews)
                            LoadChildrenOfPerson(childInfo.childId);
                        }
                        else
                        {
                            Debug.LogWarning($"[FamilyDAGBuilder] LoadSiblingsFromParent: Failed to load sibling {childInfo.childId}");
                        }
                    }
                }
            }
            
            return siblingsLoaded;
        }

        /// <summary>
        /// Load aunts and uncles (siblings of parents) and their families
        /// </summary>
        private void LoadAuntsUnclesAndFamily(int personId)
        {
            var parentageList = _dataProvider.GetParents(personId);
            foreach (var parentage in parentageList)
            {
                // Load aunts/uncles through father
                if (parentage.fatherId != 0)
                {
                    LoadSiblingsFromParent(parentage.fatherId, parentage.fatherId); // Load father's siblings
                }
                
                // Load aunts/uncles through mother
                if (parentage.motherId != 0)
                {
                    LoadSiblingsFromParent(parentage.motherId, parentage.motherId); // Load mother's siblings
                }
            }
        }

        /// <summary>
        /// Load nieces and nephews (children of siblings)
        /// </summary>
        private void LoadNiecesNephews(int personId)
        {
            // This is already handled in LoadSiblingsAndFamily when we load children of siblings
            // No additional loading needed here
        }

        /// <summary>
        /// Load cousins (children of aunts and uncles)
        /// </summary>
        private void LoadCousins(int personId)
        {
            // This is already handled in LoadAuntsUnclesAndFamily when we load children of aunts/uncles
            // No additional loading needed here
        }

        /// <summary>
        /// Load all children of a person
        /// </summary>
        private void LoadChildrenOfPerson(int personId)
        {
            var person = _familyDAG.People.ContainsKey(personId) ? _familyDAG.People[personId] : null;
            if (person == null) return;

            var marriages = _dataProvider.GetMarriages(personId, person.gender == PersonGenderType.Male);
            
            foreach (var marriage in marriages)
            {
                var children = _dataProvider.GetChildren(marriage.familyId);
                
                foreach (var childInfo in children)
                {
                    var child = LoadPersonIntoDAG(childInfo.childId);
                    if (child != null)
                    {
                        // Create parent-child relationship
                        if (person.gender == PersonGenderType.Male)
                        {
                            AddRelationshipSafely(personId, childInfo.childId, PersonRelationshipType.Father);
                        }
                        else
                        {
                            AddRelationshipSafely(personId, childInfo.childId, PersonRelationshipType.Mother);
                        }
                        
                        // Load spouse(s) of this child
                        LoadSpousesForPerson(childInfo.childId, child);
                    }
                    else
                    {
                        Debug.LogWarning($"[FamilyDAGBuilder] LoadChildrenOfPerson: Failed to load child {childInfo.childId}");
                    }
                }
            }
        }

        /// <summary>
        /// Build sibling relationships between people who share parents
        /// </summary>
        private void BuildSiblingRelationships()
        {
            var peopleByParents = new Dictionary<string, List<int>>();

            // Group people by their parents
            foreach (var personId in _familyDAG.People.Keys)
            {
                var parentage = _dataProvider.GetParents(personId);
                foreach (var parents in parentage)
                {
                    var parentKey = $"{parents.fatherId}_{parents.motherId}";
                    if (!peopleByParents.ContainsKey(parentKey))
                        peopleByParents[parentKey] = new List<int>();
                    
                    peopleByParents[parentKey].Add(personId);
                }
            }

            // Create sibling relationships
            foreach (var siblings in peopleByParents.Values)
            {
                if (siblings.Count > 1)
                {
                    for (int i = 0; i < siblings.Count; i++)
                    {
                        for (int j = i + 1; j < siblings.Count; j++)
                        {
                            AddRelationshipSafely(siblings[i], siblings[j], PersonRelationshipType.Sibling);
                            AddRelationshipSafely(siblings[j], siblings[i], PersonRelationshipType.Sibling);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Build extended relationships (aunts, uncles, cousins, etc.)
        /// This demonstrates the power of the DAG for relationship calculation
        /// </summary>
        private void BuildExtendedRelationships()
        {
            foreach (var personId in _familyDAG.People.Keys.ToList())
            {
                BuildAuntUncleRelationships(personId);
                BuildNieceNephewRelationships(personId);
                BuildCousinRelationships(personId);
            }
        }

        private void BuildAuntUncleRelationships(int personId)
        {
            // Get person's parents
            var parents = GetParents(personId);
            foreach (var parent in parents)
            {
                // Get parent's siblings (aunts/uncles)
                var parentSiblings = GetSiblings(parent.dataBaseOwnerId);
                foreach (var auntUncle in parentSiblings)
                {
                    AddRelationshipSafely(personId, auntUncle.dataBaseOwnerId, PersonRelationshipType.AuntUncle);
                    AddRelationshipSafely(auntUncle.dataBaseOwnerId, personId, PersonRelationshipType.NieceNephew);
                }
            }
        }

        private void BuildNieceNephewRelationships(int personId)
        {
            // Get person's siblings
            var siblings = GetSiblings(personId);
            
            foreach (var sibling in siblings)
            {
                // Get sibling's children (nieces/nephews)
                var siblingChildren = GetChildren(sibling.dataBaseOwnerId);
                
                foreach (var nieceNephew in siblingChildren)
                {
                    AddRelationshipSafely(personId, nieceNephew.dataBaseOwnerId, PersonRelationshipType.AuntUncle);
                    AddRelationshipSafely(nieceNephew.dataBaseOwnerId, personId, PersonRelationshipType.NieceNephew);
                }
            }
        }

        private void BuildCousinRelationships(int personId)
        {
            // Get person's parents
            var parents = GetParents(personId);
            foreach (var parent in parents)
            {
                // Get parent's siblings
                var parentSiblings = GetSiblings(parent.dataBaseOwnerId);
                foreach (var auntUncle in parentSiblings)
                {
                    // Get aunt/uncle's children (cousins)
                    var cousins = GetChildren(auntUncle.dataBaseOwnerId);
                    foreach (var cousin in cousins)
                    {
                        AddRelationshipSafely(personId, cousin.dataBaseOwnerId, PersonRelationshipType.Cousin);
                        AddRelationshipSafely(cousin.dataBaseOwnerId, personId, PersonRelationshipType.Cousin);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to add relationships safely without duplicates
        /// </summary>
        private void AddRelationshipSafely(int fromPersonId, int toPersonId, PersonRelationshipType relationshipType, int? eventDate = null)
        {
            var relationshipKey = (fromPersonId, toPersonId, relationshipType);
            if (_processedRelationships.Contains(relationshipKey))
            {
                return;
            }

            _familyDAG.AddRelationship(fromPersonId, toPersonId, relationshipType, eventDate);
            _processedRelationships.Add(relationshipKey);
        }

        // Helper methods to get relationships from the DAG
        private List<Person> GetParents(int personId)
        {
            var parents = new List<Person>();
            var edges = _familyDAG.GetDirectRelationships(personId);
            
            foreach (var edge in edges)
            {
                if ((edge.RelationshipType == PersonRelationshipType.Father || edge.RelationshipType == PersonRelationshipType.Mother)
                    && edge.ToPersonId == personId)
                {
                    parents.Add(_familyDAG.People[edge.FromPersonId]);
                }
            }
            return parents;
        }

        private List<Person> GetChildren(int personId)
        {
            var children = new List<Person>();
            var edges = _familyDAG.GetDirectRelationships(personId);
            
            foreach (var edge in edges)
            {
                // Look for Child edges (from person to child)
                if (edge.RelationshipType == PersonRelationshipType.Child && edge.FromPersonId == personId)
                {
                    children.Add(_familyDAG.People[edge.ToPersonId]);
                }
                
                // Look for Father/Mother edges (from person to child) - these are the parent-child relationships we create
                if ((edge.RelationshipType == PersonRelationshipType.Father || edge.RelationshipType == PersonRelationshipType.Mother) 
                    && edge.FromPersonId == personId)
                {
                    children.Add(_familyDAG.People[edge.ToPersonId]);
                }
            }
            
            return children;
        }

        private List<Person> GetSiblings(int personId)
        {
            var siblings = new List<Person>();
            var edges = _familyDAG.GetDirectRelationships(personId);
            
            foreach (var edge in edges)
            {
                if (edge.RelationshipType == PersonRelationshipType.Sibling && edge.FromPersonId == personId)
                {
                    siblings.Add(_familyDAG.People[edge.ToPersonId]);
                }
            }
            
            return siblings;
        }

        /// <summary>
        /// Get people organized by generation for backward compatibility
        /// This allows you to gradually migrate from the generation-based approach
        /// </summary>
        public Dictionary<int, List<Person>> GetPeopleByGeneration(int rootPersonId)
        {
            var peopleByGeneration = new Dictionary<int, List<Person>>();
            var visited = new HashSet<int>();
            
            AssignGenerations(rootPersonId, 0, peopleByGeneration, visited);
            return peopleByGeneration;
        }

        private void AssignGenerations(int personId, int generation, Dictionary<int, List<Person>> peopleByGeneration, HashSet<int> visited)
        {
            if (visited.Contains(personId) || !_familyDAG.People.ContainsKey(personId))
                return;

            visited.Add(personId);
            var person = _familyDAG.People[personId];
            person.generation = generation; // Update the person's generation

            if (!peopleByGeneration.ContainsKey(generation))
                peopleByGeneration[generation] = new List<Person>();
            
            peopleByGeneration[generation].Add(person);

            // Assign generations to children (generation + 1)
            var children = GetChildren(personId);
            foreach (var child in children)
            {
                AssignGenerations(child.dataBaseOwnerId, generation + 1, peopleByGeneration, visited);
            }

            // Assign generations to parents (generation - 1)
            var parents = GetParents(personId);
            foreach (var parent in parents)
            {
                AssignGenerations(parent.dataBaseOwnerId, generation - 1, peopleByGeneration, visited);
            }
        }
    }
} 