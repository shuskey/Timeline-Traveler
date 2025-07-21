using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using Assets.Scripts.ServiceProviders;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;

namespace Assets.Scripts.ContentProviders
{
    public class FamilyHappeningsContent
    {
        private RootsMagicFamilyHistoryDataProvider _dataProvider;
        private FamilyDAG _familyDAG;
        private bool showDataBaseOwnerId = false; // Flag to show database ID after names
        
        public FamilyHappeningsContent()
        {
            _dataProvider = new RootsMagicFamilyHistoryDataProvider();
        }

        /// <summary>
        /// Set the Family DAG for efficient relationship queries
        /// </summary>
        public void SetFamilyDAG(FamilyDAG familyDAG)
        {
            _familyDAG = familyDAG;
            Debug.Log($"[FamilyHappeningsContent] DAG set successfully. DAG contains {_familyDAG?.People?.Count ?? 0} people");
            
            // Validate DAG setup
            ValidateDAGSetup();
        }
        
        /// <summary>
        /// Validate DAG setup and compare with legacy methods
        /// </summary>
        private void ValidateDAGSetup()
        {
            if (_familyDAG == null)
            {
                Debug.LogWarning("[FamilyHappeningsContent] DAG is null - will use legacy methods");
                return;
            }
            
            Debug.Log($"[FamilyHappeningsContent] === DAG VALIDATION ===");
            Debug.Log($"[FamilyHappeningsContent] Total people in DAG: {_familyDAG.People.Count}");
            
            // Check if DAG has any relationships
            int totalRelationships = 0;
            foreach (var person in _familyDAG.People.Values)
            {
                var relationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
                totalRelationships += relationships.Count;
            }
            Debug.Log($"[FamilyHappeningsContent] Total relationships in DAG: {totalRelationships}");
            
            // Test a few sample relationships if we have people
            if (_familyDAG.People.Count > 0)
            {
                var samplePerson = _familyDAG.People.Values.First();
                Debug.Log($"[FamilyHappeningsContent] Sample person: {samplePerson.givenName} {samplePerson.surName} (ID: {samplePerson.dataBaseOwnerId})");
                
                var sampleRelationships = _familyDAG.GetDirectRelationships(samplePerson.dataBaseOwnerId);
                Debug.Log($"[FamilyHappeningsContent] Sample person has {sampleRelationships.Count} direct relationships");
                
                foreach (var rel in sampleRelationships.Take(3)) // Show first 3 relationships
                {
                    var relatedPerson = _familyDAG.People.ContainsKey(rel.ToPersonId) 
                        ? _familyDAG.People[rel.ToPersonId] 
                        : null;
                    if (relatedPerson != null)
                    {
                        Debug.Log($"[FamilyHappeningsContent]   -> {relatedPerson.givenName} {relatedPerson.surName} ({rel.RelationshipType})");
                    }
                }
                
                // Test relationship determination for a few sample pairs
                TestRelationshipDetermination();
            }
            
            Debug.Log($"[FamilyHappeningsContent] === END DAG VALIDATION ===");
        }
        
        /// <summary>
        /// Test relationship determination between sample people to validate DAG vs legacy
        /// </summary>
        private void TestRelationshipDetermination()
        {
            Debug.Log($"[FamilyHappeningsContent] === TESTING RELATIONSHIP DETERMINATION ===");
            
            var people = _familyDAG.People.Values.Take(5).ToList(); // Test first 5 people
            
            for (int i = 0; i < people.Count; i++)
            {
                for (int j = i + 1; j < people.Count; j++)
                {
                    var person1 = people[i];
                    var person2 = people[j];
                    
                    // Test DAG relationship
                    var dagRelationship = _familyDAG.GetRelationshipBetween(person1.dataBaseOwnerId, person2.dataBaseOwnerId);
                    
                    // Test legacy relationship (temporarily disable DAG to force legacy)
                    var originalDAG = _familyDAG;
                    _familyDAG = null;
                    var legacyRelationship = GetRelationshipToPerson(person1, person2);
                    _familyDAG = originalDAG;
                    
                    Debug.Log($"[FamilyHappeningsContent] {person1.givenName} -> {person2.givenName}: DAG='{dagRelationship}' vs Legacy='{legacyRelationship}' {(dagRelationship == legacyRelationship ? "✓" : "✗")}");
                }
            }
            
            Debug.Log($"[FamilyHappeningsContent] === END RELATIONSHIP TESTING ===");
        }
        
        public void Initialize()
        {
                    string rootsMagicFileName = DataObjects.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;
        var config = new Dictionary<string, string>
        {
            { DataObjects.PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, rootsMagicFileName }
            };
            _dataProvider.Initialize(config);
        }

        /// <summary>
        /// Generates Family Happenings content for a focus person and year
        /// </summary>
        /// <param name="focusPerson">The person to center the report around</param>
        /// <param name="year">The year to generate happenings for</param>
        /// <returns>Formatted Family Happenings report</returns>
        public string GetFamilyHappeningsContent(Person focusPerson, int year)
        {
           // Debug.Log($"[FamilyHappeningsContent] Starting generation for {focusPerson.givenName} {focusPerson.surName} (ID: {focusPerson.dataBaseOwnerId}) for year {year}");
            
            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("=== FAMILY HAPPENINGS ===");
            var focusPersonAge = CalculateAge(focusPerson.originalBirthEventDateYear, year);
            sb.AppendLine($"For: {FormatPersonName(focusPerson)} (age {focusPersonAge.Replace(" years old", "").Replace("infant", "0")})");
            sb.AppendLine();
            
            // Get close family members
            //Debug.Log($"[FamilyHappeningsContent] Getting close family members...");
            var closeFamilyMembers = GetCloseFamilyMembers(focusPerson);
            //Debug.Log($"[FamilyHappeningsContent] Found {closeFamilyMembers.Count} close family members");
            
            // Generate each section
            sb.AppendLine("BIRTH ANNOUNCEMENTS");
            sb.AppendLine("===================");
            sb.AppendLine(GenerateBirthAnnouncements(focusPerson, closeFamilyMembers, year));
            sb.AppendLine();
            
            sb.AppendLine("MARRIAGE ANNOUNCEMENTS");
            sb.AppendLine("======================");
            sb.AppendLine(GenerateMarriageAnnouncements(focusPerson, closeFamilyMembers, year));
            sb.AppendLine();
            
            sb.AppendLine("DEATH ANNOUNCEMENTS");
            sb.AppendLine("===================");
            sb.AppendLine(GenerateDeathAnnouncements(focusPerson, closeFamilyMembers, year));
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats a person's name with optional database ID
        /// </summary>
        /// <param name="person">The person to format</param>
        /// <returns>Formatted name string</returns>
        private string FormatPersonName(Person person)
        {
            if (person == null) return "Unknown";
            
            string name = $"{person.givenName} {person.surName}";
            if (showDataBaseOwnerId)
                name += $" ({person.dataBaseOwnerId})";
            
            return name;
        }

        /// <summary>
        /// Gets all close family members for the focus person
        /// </summary>
        private List<Person> GetCloseFamilyMembers(Person focusPerson)
        {
            var closeFamilyMembers = new HashSet<Person>();
            
            // Add the focus person themselves
            closeFamilyMembers.Add(focusPerson);
            
            // Use DAG for efficient family gathering if available
            if (_familyDAG != null)
            {
                Debug.Log($"[FamilyHappeningsContent] === USING DAG FOR FAMILY GATHERING ===");
                Debug.Log($"[FamilyHappeningsContent] Focus person: {focusPerson.givenName} {focusPerson.surName} (ID: {focusPerson.dataBaseOwnerId})");
                
                // Get all descendants (children, grandchildren, etc.)
                var descendants = _familyDAG.GetDescendants(focusPerson.dataBaseOwnerId, 3);
                Debug.Log($"[FamilyHappeningsContent] DAG found {descendants.Count} descendants");
                foreach (var descendant in descendants)
                {
                    closeFamilyMembers.Add(descendant);
                    Debug.Log($"[FamilyHappeningsContent]   + Descendant: {descendant.givenName} {descendant.surName} (ID: {descendant.dataBaseOwnerId})");
                }
                
                // Get all ancestors (parents, grandparents, etc.)
                var ancestors = _familyDAG.GetAncestors(focusPerson.dataBaseOwnerId, 2);
                Debug.Log($"[FamilyHappeningsContent] DAG found {ancestors.Count} ancestors");
                foreach (var ancestor in ancestors)
                {
                    closeFamilyMembers.Add(ancestor);
                    Debug.Log($"[FamilyHappeningsContent]   + Ancestor: {ancestor.givenName} {ancestor.surName} (ID: {ancestor.dataBaseOwnerId})");
                }
                
                // Get direct relationships (spouses, siblings, etc.)
                var directRelationships = _familyDAG.GetDirectRelationships(focusPerson.dataBaseOwnerId);
                Debug.Log($"[FamilyHappeningsContent] DAG found {directRelationships.Count} direct relationships");
                foreach (var relationship in directRelationships)
                {
                    var relatedPerson = _familyDAG.People.ContainsKey(relationship.ToPersonId) 
                        ? _familyDAG.People[relationship.ToPersonId] 
                        : null;
                    if (relatedPerson != null)
                    {
                        closeFamilyMembers.Add(relatedPerson);
                        Debug.Log($"[FamilyHappeningsContent]   + Direct: {relatedPerson.givenName} {relatedPerson.surName} ({relationship.RelationshipType})");
                    }
                }
                
                Debug.Log($"[FamilyHappeningsContent] DAG gathered {closeFamilyMembers.Count} total close family members");
                Debug.Log($"[FamilyHappeningsContent] === END DAG FAMILY GATHERING ===");
            }
            else
            {
                // Fallback to legacy complex method
                Debug.Log($"[FamilyHappeningsContent] === USING LEGACY FAMILY GATHERING ===");
                Debug.Log($"[FamilyHappeningsContent] DAG not available, using legacy methods");
                return GetCloseFamilyMembersLegacy(focusPerson);
            }
            
            return closeFamilyMembers.ToList();
        }

        /// <summary>
        /// Legacy close family member gathering - kept as fallback
        /// </summary>
        private List<Person> GetCloseFamilyMembersLegacy(Person focusPerson)
        {
            var closeFamilyMembers = new HashSet<Person>();
            
            // Add the focus person themselves
            closeFamilyMembers.Add(focusPerson);
            
            // Get all descendants of the focus person
            AddDescendants(focusPerson, closeFamilyMembers);
            
            // Get spouse(s) of the focus person
            AddSpouses(focusPerson, closeFamilyMembers);
            
            // Get in-laws (spouse's family)
            AddInLaws(focusPerson, closeFamilyMembers);
            
            // Get siblings and their spouses
            AddSiblingsAndTheirSpouses(focusPerson, closeFamilyMembers);
            
            // Get nieces and nephews
            AddNiecesAndNephews(focusPerson, closeFamilyMembers);
            
            // Get parents
            AddParents(focusPerson, closeFamilyMembers);
            
            // Get grandparents
            AddGrandparents(focusPerson, closeFamilyMembers);
            
            // Get aunts, uncles, and cousins
            AddAuntsUnclesAndCousins(focusPerson, closeFamilyMembers);
            
            return closeFamilyMembers.ToList();
        }

        /// <summary>
        /// Gets tight family members for the focus person (descendants, siblings, parents)
        /// </summary>
        private List<Person> GetTightFamilyMembers(Person focusPerson)
        {
            var tightFamilyMembers = new HashSet<Person>();
            
            // Add the focus person themselves
            tightFamilyMembers.Add(focusPerson);
            
            // Get all descendants of the focus person
            AddDescendants(focusPerson, tightFamilyMembers);
            
            // Get siblings
            AddSiblings(focusPerson, tightFamilyMembers);
            
            // Get parents
            AddParents(focusPerson, tightFamilyMembers);
            
            return tightFamilyMembers.ToList();
        }

        /// <summary>
        /// Generates birth announcements for the given year
        /// </summary>
        private string GenerateBirthAnnouncements(Person focusPerson, List<Person> closeFamilyMembers, int year)
        {
            var sb = new StringBuilder();
            var birthsThisYear = closeFamilyMembers.Where(p => p.originalBirthEventDateYear == year)
                .GroupBy(p => p.dataBaseOwnerId)
                .Select(g => g.First())
                .ToList();
            
            if (!birthsThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var child in birthsThisYear)
            {
                var parents = GetParentsOfPerson(child);
                var relationship = GetRelationshipToPerson(child, focusPerson);
                
                // Check if focus person is one of the parents
                bool focusPersonIsParent = parents.Any(p => p.dataBaseOwnerId == focusPerson.dataBaseOwnerId);
                
                // Handle focus person's own birth differently
                if (child.dataBaseOwnerId == focusPerson.dataBaseOwnerId)
                {
                    sb.AppendLine($"Mr. and Mrs. {GetParentsNames(parents)} are pleased to announce the birth of their {GetGenderText(child.gender)}, {FormatPersonName(child)}.");
                    sb.AppendLine($"Born: {FormatDate(child.originalBirthEventDateMonth, child.originalBirthEventDateDay, child.originalBirthEventDateYear)}");
                }
                else
                {
                    sb.AppendLine($"Mr. and Mrs. {GetParentsNames(parents)} are pleased to announce the birth of their {GetGenderText(child.gender)}, {FormatPersonName(child)}.");
                    
                    // Only show relationship if focus person is not the parent
                    if (!focusPersonIsParent)
                    {
                        sb.AppendLine($"{child.givenName} is the {relationship} of {FormatPersonName(focusPerson)}.");
                    }
                    
                    sb.AppendLine($"Born: {FormatDate(child.originalBirthEventDateMonth, child.originalBirthEventDateDay, child.originalBirthEventDateYear)}");
                }
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates marriage announcements for the given year
        /// </summary>
        private string GenerateMarriageAnnouncements(Person focusPerson, List<Person> closeFamilyMembers, int year)
        {
            var sb = new StringBuilder();
            var marriagesThisYear = new List<Marriage>();
            
            // Get all marriages for close family members in this year
            foreach (var person in closeFamilyMembers)
            {
                var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
                marriagesThisYear.AddRange(marriages.Where(m => m.marriageYear == year));
                
                var marriagesAsWife = _dataProvider.GetMarriages(person.dataBaseOwnerId, false);
                marriagesThisYear.AddRange(marriagesAsWife.Where(m => m.marriageYear == year));
            }
            
            // Remove duplicates - group by both husband and wife IDs to ensure no duplicate marriages
            marriagesThisYear = marriagesThisYear
                .GroupBy(m => new { m.husbandId, m.wifeId })
                .Select(g => g.First())
                .ToList();
            
            if (!marriagesThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var marriage in marriagesThisYear)
            {
                var bride = _dataProvider.GetPerson(marriage.wifeId).FirstOrDefault();
                var groom = _dataProvider.GetPerson(marriage.husbandId).FirstOrDefault();
                
                if (bride != null && groom != null)
                {
                    var brideRelationship = GetRelationshipToPerson(bride, focusPerson);
                    var groomRelationship = GetRelationshipToPerson(groom, focusPerson);
                    
                    var brideAge = CalculateAge(bride.originalBirthEventDateYear, marriage.marriageYear);
                    var groomAge = CalculateAge(groom.originalBirthEventDateYear, marriage.marriageYear);
                    
                    sb.AppendLine($"{FormatPersonName(bride)}, {brideAge}, {brideRelationship} of {FormatPersonName(focusPerson)}, was united in marriage to {FormatPersonName(groom)}, {groomAge}, {groomRelationship} of {FormatPersonName(focusPerson)}.");
                    sb.AppendLine($"The ceremony took place on {FormatDate(marriage.marriageMonth, marriage.marriageDay, marriage.marriageYear)}.");
                    
                    // Add parents information
                    var brideParents = GetParentsOfPerson(bride);
                    var groomParents = GetParentsOfPerson(groom);
                    
                    if (brideParents.Any())
                        sb.AppendLine($"The bride is the daughter of {GetParentsNames(brideParents)}.");
                    if (groomParents.Any())
                        sb.AppendLine($"The groom is the son of {GetParentsNames(groomParents)}.");
                    
                    sb.AppendLine();
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates death announcements for the given year
        /// </summary>
        private string GenerateDeathAnnouncements(Person focusPerson, List<Person> closeFamilyMembers, int year)
        {
            var sb = new StringBuilder();
            var deathsThisYear = closeFamilyMembers.Where(p => p.originalDeathEventDateYear == year && !p.isLiving)
                .GroupBy(p => p.dataBaseOwnerId)
                .Select(g => g.First())
                .ToList();
            
            if (!deathsThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var deceased in deathsThisYear)
            {
                var relationship = GetRelationshipToPerson(deceased, focusPerson);
                var age = CalculateAge(deceased.originalBirthEventDateYear, deceased.originalDeathEventDateYear);
                
                sb.AppendLine($"{FormatPersonName(deceased)}, {age}, {relationship} of {FormatPersonName(focusPerson)}, passed away on {FormatDate(deceased.originalDeathEventDateMonth, deceased.originalDeathEventDateDay, deceased.originalDeathEventDateYear)}.");
                sb.AppendLine($"Born: {FormatDate(deceased.originalBirthEventDateMonth, deceased.originalBirthEventDateDay, deceased.originalBirthEventDateYear)}");
                
                // Get tight family of the deceased
                var tightFamily = GetTightFamilyMembers(deceased);
                
                // Those who preceded them in death
                var preceded = tightFamily.Where(p => !p.isLiving && p.originalDeathEventDateYear < deceased.originalDeathEventDateYear && p.dataBaseOwnerId != deceased.dataBaseOwnerId)
                    .GroupBy(p => p.dataBaseOwnerId)
                    .Select(g => g.First())
                    .ToList();
                if (preceded.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine($"Preceded in death by: {string.Join(", ", preceded.Select(p => $"{FormatPersonName(p)} ({GetRelationshipToPerson(p, deceased)}, {p.originalDeathEventDateYear})"))}");
                }
                
                // Those who survived them
                var survived = tightFamily.Where(p => p.isLiving && 
                    p.dataBaseOwnerId != deceased.dataBaseOwnerId &&
                    (p.originalBirthEventDateYear == 0 || p.originalBirthEventDateYear <= deceased.originalDeathEventDateYear))
                    .GroupBy(p => p.dataBaseOwnerId)
                    .Select(g => g.First())
                    .ToList();
                if (survived.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine($"Survived by: {string.Join(", ", survived.Select(p => $"{FormatPersonName(p)} ({GetRelationshipToPerson(p, deceased)})"))}");
                }
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        // Helper methods for adding family members
        private void AddDescendants(Person person, HashSet<Person> familyMembers)
        {
            AddDescendants(person, familyMembers, new HashSet<int>());
        }
        
        private void AddDescendants(Person person, HashSet<Person> familyMembers, HashSet<int> visitedPersonIds, int maxDepth = 10)
        {
            // Prevent circular references and limit recursion depth
            if (visitedPersonIds.Contains(person.dataBaseOwnerId) || maxDepth <= 0)
                return;
                
            visitedPersonIds.Add(person.dataBaseOwnerId);
            
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            marriages.AddRange(_dataProvider.GetMarriages(person.dataBaseOwnerId, false));
            
            foreach (var marriage in marriages)
            {
                var children = _dataProvider.GetChildren(marriage.familyId);
                foreach (var child in children)
                {
                    var childPerson = _dataProvider.GetPerson(child.childId).FirstOrDefault();
                    if (childPerson != null && familyMembers.Add(childPerson))
                    {
                        // Recursively add descendants with circular reference protection and depth limiting
                        AddDescendants(childPerson, familyMembers, visitedPersonIds, maxDepth - 1);
                    }
                }
            }
        }

        private void AddSpouses(Person person, HashSet<Person> familyMembers)
        {
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.wifeId).FirstOrDefault();
                if (spouse != null) familyMembers.Add(spouse);
            }
            
            marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, false);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.husbandId).FirstOrDefault();
                if (spouse != null) familyMembers.Add(spouse);
            }
        }

        private void AddInLaws(Person person, HashSet<Person> familyMembers)
        {
            // Get person's spouse(s)
            var spouses = new List<Person>();
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.wifeId).FirstOrDefault();
                if (spouse != null) spouses.Add(spouse);
            }
            
            marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, false);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.husbandId).FirstOrDefault();
                if (spouse != null) spouses.Add(spouse);
            }
            
            // For each spouse, add their parents and siblings
            foreach (var spouse in spouses)
            {
                AddParents(spouse, familyMembers);
                AddSiblings(spouse, familyMembers);
            }
        }

        private void AddSiblingsAndTheirSpouses(Person person, HashSet<Person> familyMembers)
        {
            AddSiblings(person, familyMembers);
            
            // Add spouses of siblings
            var siblings = GetSiblings(person);
            foreach (var sibling in siblings)
            {
                AddSpouses(sibling, familyMembers);
            }
        }

        private void AddSiblings(Person person, HashSet<Person> familyMembers)
        {
            var siblings = GetSiblings(person);
            foreach (var sibling in siblings)
            {
                familyMembers.Add(sibling);
            }
        }

        private void AddNiecesAndNephews(Person person, HashSet<Person> familyMembers)
        {
            var siblings = GetSiblings(person);
            foreach (var sibling in siblings)
            {
                AddDescendants(sibling, familyMembers);
            }
        }

        private void AddParents(Person person, HashSet<Person> familyMembers)
        {
            var parents = GetParentsOfPerson(person);
            foreach (var parent in parents)
            {
                familyMembers.Add(parent);
            }
        }

        private void AddGrandparents(Person person, HashSet<Person> familyMembers)
        {
            var parents = GetParentsOfPerson(person);
            foreach (var parent in parents)
            {
                var grandparents = GetParentsOfPerson(parent);
                foreach (var grandparent in grandparents)
                {
                    familyMembers.Add(grandparent);
                }
            }
        }

        private void AddAuntsUnclesAndCousins(Person person, HashSet<Person> familyMembers)
        {
            var parents = GetParentsOfPerson(person);
            foreach (var parent in parents)
            {
                // Get parent's siblings (aunts and uncles)
                var auntsUncles = GetSiblings(parent);
                foreach (var auntUncle in auntsUncles)
                {
                    familyMembers.Add(auntUncle);
                    // Add their spouses
                    AddSpouses(auntUncle, familyMembers);
                    // Add their children (cousins)
                    AddDescendants(auntUncle, familyMembers);
                }
            }
        }

        // Helper methods for getting family relationships
        private List<Person> GetSiblings(Person person)
        {
            return GetSiblings(person, new HashSet<int>());
        }
        
        private List<Person> GetSiblings(Person person, HashSet<int> visitedPersonIds)
        {
            var siblings = new List<Person>();
            
            // Prevent circular references
            if (visitedPersonIds.Contains(person.dataBaseOwnerId))
            {
                Debug.LogWarning($"[FamilyHappeningsContent] Circular reference detected getting siblings for {person.givenName} {person.surName} (ID: {person.dataBaseOwnerId})");
                return siblings;
            }
            
            visitedPersonIds.Add(person.dataBaseOwnerId);
            
            try
            {
                var parentage = _dataProvider.GetParents(person.dataBaseOwnerId);
                
                foreach (var parent in parentage)
                {
                    var children = _dataProvider.GetChildren(parent.familyId);
                    foreach (var child in children)
                    {
                        // Skip self and prevent circular references
                        if (child.childId != person.dataBaseOwnerId && !visitedPersonIds.Contains(child.childId))
                        {
                            var siblingPerson = _dataProvider.GetPerson(child.childId).FirstOrDefault();
                            if (siblingPerson != null && !siblings.Any(s => s.dataBaseOwnerId == siblingPerson.dataBaseOwnerId))
                            {
                                siblings.Add(siblingPerson);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FamilyHappeningsContent] Error getting siblings for {person.givenName} {person.surName} (ID: {person.dataBaseOwnerId}): {ex.Message}");
            }
            
            return siblings;
        }

        private List<Person> GetParentsOfPerson(Person person)
        {
            return GetParentsOfPerson(person, new HashSet<int>());
        }
        
        private List<Person> GetParentsOfPerson(Person person, HashSet<int> visitedPersonIds)
        {
            var parents = new List<Person>();
            
            // Prevent circular references - if we're already looking up this person's parents, stop
            if (visitedPersonIds.Contains(person.dataBaseOwnerId))
            {
                Debug.LogWarning($"[FamilyHappeningsContent] Circular reference detected for person {person.givenName} {person.surName} (ID: {person.dataBaseOwnerId})");
                return parents;
            }
            
            visitedPersonIds.Add(person.dataBaseOwnerId);
            
            try
            {
                var parentage = _dataProvider.GetParents(person.dataBaseOwnerId);
                
                foreach (var parent in parentage)
                {
                    // Prevent person from being their own parent
                    if (parent.fatherId == person.dataBaseOwnerId || parent.motherId == person.dataBaseOwnerId)
                    {
                        Debug.LogWarning($"[FamilyHappeningsContent] Self-parent relationship detected for person {person.givenName} {person.surName} (ID: {person.dataBaseOwnerId})");
                        continue;
                    }
                    
                    var father = _dataProvider.GetPerson(parent.fatherId).FirstOrDefault();
                    var mother = _dataProvider.GetPerson(parent.motherId).FirstOrDefault();
                    
                    if (father != null && !parents.Any(p => p.dataBaseOwnerId == father.dataBaseOwnerId))
                        parents.Add(father);
                    if (mother != null && !parents.Any(p => p.dataBaseOwnerId == mother.dataBaseOwnerId))
                        parents.Add(mother);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FamilyHappeningsContent] Error getting parents for {person.givenName} {person.surName} (ID: {person.dataBaseOwnerId}): {ex.Message}");
            }
            
            return parents;
        }

        // Helper methods for formatting and relationship calculation
        private string GetRelationshipToPerson(Person relationshipPerson, Person sourcePerson)
        {
            Debug.Log($"[FamilyHappeningsContent] === RELATIONSHIP DETERMINATION ===");
            Debug.Log($"[FamilyHappeningsContent] Finding relationship: {relationshipPerson.givenName} {relationshipPerson.surName} (ID: {relationshipPerson.dataBaseOwnerId}) to {sourcePerson.givenName} {sourcePerson.surName} (ID: {sourcePerson.dataBaseOwnerId})");
            
            if (relationshipPerson.dataBaseOwnerId == sourcePerson.dataBaseOwnerId)
            {
                Debug.Log($"[FamilyHappeningsContent] Same person - returning 'self'");
                return "self";
            }
            
            // Use DAG for efficient relationship lookup if available
            if (_familyDAG != null)
            {
                Debug.Log($"[FamilyHappeningsContent] Using DAG for relationship lookup");
                var relationship = _familyDAG.GetRelationshipBetween(relationshipPerson.dataBaseOwnerId, sourcePerson.dataBaseOwnerId);
                Debug.Log($"[FamilyHappeningsContent] DAG returned: '{relationship}'");
                
                if (relationship != "relative" && !string.IsNullOrEmpty(relationship))
                {
                    Debug.Log($"[FamilyHappeningsContent] DAG found relationship: {relationship}");
                    Debug.Log($"[FamilyHappeningsContent] === END RELATIONSHIP DETERMINATION (DAG) ===");
                    return relationship;
                }
                else
                {
                    Debug.Log($"[FamilyHappeningsContent] DAG returned generic 'relative' - falling back to legacy");
                }
            }
            else
            {
                Debug.Log($"[FamilyHappeningsContent] DAG not available - using legacy relationship determination");
            }
            
            // Fallback to legacy complex logic if DAG not available or relationship not found
            Debug.Log($"[FamilyHappeningsContent] Using legacy relationship determination");
            var legacyResult = GetRelationshipToPersonLegacy(relationshipPerson, sourcePerson);
            Debug.Log($"[FamilyHappeningsContent] Legacy returned: {legacyResult}");
            Debug.Log($"[FamilyHappeningsContent] === END RELATIONSHIP DETERMINATION (LEGACY) ===");
            return legacyResult;
        }



        /// <summary>
        /// Legacy complex relationship determination - kept as fallback
        /// </summary>
        private string GetRelationshipToPersonLegacy(Person relationshipPerson, Person sourcePerson)
        {
            // Check direct relationships first
            
            // Check if relationshipPerson is a descendant of sourcePerson
            if (IsDescendant(relationshipPerson, sourcePerson))
            {
               // Debug.Log($"[FamilyHappeningsContent] Found descendant relationship");
                return GetDescendantRelationship(relationshipPerson, sourcePerson);
            }
            
            // Check if relationshipPerson is an ancestor of sourcePerson
            if (IsAncestor(relationshipPerson, sourcePerson))
            {
               // Debug.Log($"[FamilyHappeningsContent] Found ancestor relationship");
                return GetAncestorRelationship(relationshipPerson, sourcePerson);
            }
            
            // Check if relationshipPerson is a sibling of sourcePerson
            if (IsSibling(relationshipPerson, sourcePerson))
            {
               // Debug.Log($"[FamilyHappeningsContent] Found sibling relationship");
                return GetSiblingRelationship(relationshipPerson, sourcePerson);
            }
            
            // Check if relationshipPerson is a spouse of sourcePerson
            if (IsSpouse(relationshipPerson, sourcePerson))
            {
               // Debug.Log($"[FamilyHappeningsContent] Found spouse relationship");
                return GetSpouseRelationship(relationshipPerson, sourcePerson);
            }
            
            // Check extended relationships
            
            // Check for in-laws
            string inLawRelationship = GetInLawRelationship(relationshipPerson, sourcePerson);
            if (inLawRelationship != null)
            {
                    Debug.Log($"[FamilyHappeningsContent] Found in-law relationship: {inLawRelationship}");
                return inLawRelationship;
            }
            
            // Check for aunt/uncle relationships
            string auntUncleRelationship = GetAuntUncleRelationship(relationshipPerson, sourcePerson);
            if (auntUncleRelationship != null)
            {
               // Debug.Log($"[FamilyHappeningsContent] Found aunt/uncle relationship: {auntUncleRelationship}");
                return auntUncleRelationship;
            }
            
            // Check for niece/nephew relationships
            // Debug.Log($"[FamilyHappeningsContent] Checking niece/nephew relationship...");
            string nieceNephewRelationship = GetNieceNephewRelationship(relationshipPerson, sourcePerson);
            if (nieceNephewRelationship != null)
            {
               // Debug.Log($"[FamilyHappeningsContent] Found niece/nephew relationship: {nieceNephewRelationship}");
                return nieceNephewRelationship;
            }
            
            // Check for cousin relationships
            //Debug.Log($"[FamilyHappeningsContent] Checking cousin relationship...");
            string cousinRelationship = GetCousinRelationship(relationshipPerson, sourcePerson);
            if (cousinRelationship != null)
            {
               // Debug.Log($"[FamilyHappeningsContent] Found cousin relationship: {cousinRelationship}");
                return cousinRelationship;
            }
            
            // Default to a generic relationship
            Debug.Log($"[FamilyHappeningsContent] No specific relationship found, defaulting to 'relative'");
            return "relative";
        }

        // Basic relationship checking methods (simplified)
        private bool IsDescendant(Person person, Person ancestor)
        {
            return IsDescendant(person, ancestor, new HashSet<int>());
        }
        
        private bool IsDescendant(Person person, Person ancestor, HashSet<int> visitedPersonIds, int maxDepth = 10)
        {
            // Prevent circular references and limit recursion depth
            if (visitedPersonIds.Contains(person.dataBaseOwnerId) || maxDepth <= 0)
                return false;
                
            visitedPersonIds.Add(person.dataBaseOwnerId);
            
            // Check if person is a child, grandchild, etc. of ancestor
            var parents = GetParentsOfPerson(person, visitedPersonIds);
            foreach (var parent in parents)
            {
                if (parent.dataBaseOwnerId == ancestor.dataBaseOwnerId)
                    return true;
                if (IsDescendant(parent, ancestor, visitedPersonIds, maxDepth - 1))
                    return true;
            }
            return false;
        }

        private bool IsAncestor(Person person, Person descendant)
        {
            return IsDescendant(descendant, person);
        }

        private bool IsSibling(Person person, Person otherPerson)
        {
            var person1Parents = GetParentsOfPerson(person);
            var person2Parents = GetParentsOfPerson(otherPerson);
            
            return person1Parents.Any(p1 => person2Parents.Any(p2 => p1.dataBaseOwnerId == p2.dataBaseOwnerId));
        }

        private bool IsSpouse(Person person, Person otherPerson)
        {
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            if (marriages.Any(m => m.wifeId == otherPerson.dataBaseOwnerId))
                return true;
            
            marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, false);
            if (marriages.Any(m => m.husbandId == otherPerson.dataBaseOwnerId))
                return true;
            
            return false;
        }

        private string GetDescendantRelationship(Person descendant, Person ancestor)
        {
            return GetDescendantRelationship(descendant, ancestor, new HashSet<int>());
        }
        
        private string GetDescendantRelationship(Person descendant, Person ancestor, HashSet<int> visitedPersonIds)
        {
            // Prevent circular references
            if (visitedPersonIds.Contains(descendant.dataBaseOwnerId))
                return "descendant";
                
            visitedPersonIds.Add(descendant.dataBaseOwnerId);
            
            var parents = GetParentsOfPerson(descendant, visitedPersonIds);
            if (parents.Any(p => p.dataBaseOwnerId == ancestor.dataBaseOwnerId))
            {
                return descendant.gender == PersonGenderType.Male ? "son" : "daughter";
            }
            
            // Check for grandchild, great-grandchild, etc.
            foreach (var parent in parents)
            {
                if (IsDescendant(parent, ancestor))
                {
                    var parentRelationship = GetDescendantRelationship(parent, ancestor, visitedPersonIds);
                    if (parentRelationship == "son" || parentRelationship == "daughter")
                    {
                        return descendant.gender == PersonGenderType.Male ? "grandson" : "granddaughter";
                    }
                }
            }
            
            return "descendant";
        }

        private string GetAncestorRelationship(Person ancestor, Person descendant)
        {
            return GetAncestorRelationship(ancestor, descendant, new HashSet<int>());
        }
        
        private string GetAncestorRelationship(Person ancestor, Person descendant, HashSet<int> visitedPersonIds)
        {
            // Prevent circular references
            if (visitedPersonIds.Contains(descendant.dataBaseOwnerId))
                return "ancestor";
                
            visitedPersonIds.Add(descendant.dataBaseOwnerId);
            
            var parents = GetParentsOfPerson(descendant, visitedPersonIds);
            if (parents.Any(p => p.dataBaseOwnerId == ancestor.dataBaseOwnerId))
            {
                return ancestor.gender == PersonGenderType.Male ? "father" : "mother";
            }
            
            // Check for grandfather, grandmother, etc.
            foreach (var parent in parents)
            {
                if (IsAncestor(ancestor, parent))
                {
                    var parentRelationship = GetAncestorRelationship(ancestor, parent, visitedPersonIds);
                    if (parentRelationship == "father" || parentRelationship == "mother")
                    {
                        return ancestor.gender == PersonGenderType.Male ? "grandfather" : "grandmother";
                    }
                }
            }
            
            return "ancestor";
        }

        private string GetSiblingRelationship(Person sibling, Person person)
        {
            return sibling.gender == PersonGenderType.Male ? "brother" : "sister";
        }

        private string GetSpouseRelationship(Person spouse, Person person)
        {
            return spouse.gender == PersonGenderType.Male ? "husband" : "wife";
        }

        // Extended relationship methods
        private string GetInLawRelationship(Person relationshipPerson, Person sourcePerson)
        {
            // Check if relationshipPerson is parent of sourcePerson's spouse (mother/father-in-law)
            var sourceSpouses = GetSpousesOfPerson(sourcePerson);
            foreach (var spouse in sourceSpouses)
            {
                var spouseParents = GetParentsOfPerson(spouse);
                if (spouseParents.Any(p => p.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                {
                    return relationshipPerson.gender == PersonGenderType.Male ? "father-in-law" : "mother-in-law";
                }
            }
            
            // Check if relationshipPerson is sibling of sourcePerson's spouse (sister/brother-in-law)
            foreach (var spouse in sourceSpouses)
            {
                var spouseSiblings = GetSiblings(spouse);
                if (spouseSiblings.Any(s => s.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                {
                    return relationshipPerson.gender == PersonGenderType.Male ? "brother-in-law" : "sister-in-law";
                }
            }
            
            // Check if relationshipPerson is spouse of sourcePerson's sibling (sister/brother-in-law)
            var sourceSiblings = GetSiblings(sourcePerson);
            foreach (var sibling in sourceSiblings)
            {
                var siblingSpouses = GetSpousesOfPerson(sibling);
                if (siblingSpouses.Any(s => s.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                {
                    return relationshipPerson.gender == PersonGenderType.Male ? "brother-in-law" : "sister-in-law";
                }
            }
            
            return null;
        }

        private string GetAuntUncleRelationship(Person relationshipPerson, Person sourcePerson)
        {
            // Check if relationshipPerson is sibling of sourcePerson's parent (aunt/uncle)
            var sourceParents = GetParentsOfPerson(sourcePerson);
            foreach (var parent in sourceParents)
            {
                var parentSiblings = GetSiblings(parent);
                if (parentSiblings.Any(s => s.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                {
                    return relationshipPerson.gender == PersonGenderType.Male ? "uncle" : "aunt";
                }
            }
            
            // Check if relationshipPerson is spouse of sourcePerson's parent's sibling (aunt/uncle by marriage)
            foreach (var parent in sourceParents)
            {
                var parentSiblings = GetSiblings(parent);
                foreach (var parentSibling in parentSiblings)
                {
                    var parentSiblingSpouses = GetSpousesOfPerson(parentSibling);
                    if (parentSiblingSpouses.Any(s => s.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                    {
                        return relationshipPerson.gender == PersonGenderType.Male ? "uncle" : "aunt";
                    }
                }
            }
            
            return null;
        }

        private string GetNieceNephewRelationship(Person relationshipPerson, Person sourcePerson)
        {
            // Check if relationshipPerson is child of sourcePerson's sibling (niece/nephew)
            var sourceSiblings = GetSiblings(sourcePerson);
            //Debug.Log($"[FamilyHappeningsContent] GetNieceNephewRelationship: Checking {relationshipPerson.givenName} {relationshipPerson.surName} against {sourcePerson.givenName} {sourcePerson.surName}. Source has {sourceSiblings.Count} siblings.");
            
            foreach (var sibling in sourceSiblings)
            {
                //Debug.Log($"[FamilyHappeningsContent] Checking sibling: {sibling.givenName} {sibling.surName}");
                if (IsDescendant(relationshipPerson, sibling))
                {
                    //Debug.Log($"[FamilyHappeningsContent] {relationshipPerson.givenName} is descendant of {sibling.givenName}");
                    // Check if it's a direct child (niece/nephew) vs grandchild, etc.
                    var siblingChildren = GetChildrenOfPerson(sibling);
                    //Debug.Log($"[FamilyHappeningsContent] Sibling {sibling.givenName} has {siblingChildren.Count} children");
                    if (siblingChildren.Any(c => c.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                    {
                        //Debug.Log($"[FamilyHappeningsContent] Found niece/nephew relationship!");
                        return relationshipPerson.gender == PersonGenderType.Male ? "nephew" : "niece";
                    }
                }
            }
            
            return null;
        }

        private string GetCousinRelationship(Person relationshipPerson, Person sourcePerson)
        {
            // Check if relationshipPerson is child of sourcePerson's parent's sibling (cousin)
            var sourceParents = GetParentsOfPerson(sourcePerson);
            //Debug.Log($"[FamilyHappeningsContent] GetCousinRelationship: Checking {relationshipPerson.givenName} {relationshipPerson.surName} against {sourcePerson.givenName} {sourcePerson.surName}. Source has {sourceParents.Count} parents.");
            
            foreach (var parent in sourceParents)
            {
                //Debug.Log($"[FamilyHappeningsContent] Checking parent: {parent.givenName} {parent.surName}");
                var parentSiblings = GetSiblings(parent);
                //Debug.Log($"[FamilyHappeningsContent] Parent {parent.givenName} has {parentSiblings.Count} siblings");
                
                foreach (var parentSibling in parentSiblings)
                {
                    //Debug.Log($"[FamilyHappeningsContent] Checking parent sibling (aunt/uncle): {parentSibling.givenName} {parentSibling.surName}");
                    var parentSiblingChildren = GetChildrenOfPerson(parentSibling);
                    //Debug.Log($"[FamilyHappeningsContent] Aunt/Uncle {parentSibling.givenName} has {parentSiblingChildren.Count} children");
                    
                    if (parentSiblingChildren.Any(c => c.dataBaseOwnerId == relationshipPerson.dataBaseOwnerId))
                    {
                        //Debug.Log($"[FamilyHappeningsContent] Found cousin relationship!");
                        return "cousin";
                    }
                }
            }
            
            return null;
        }

        // Helper method to get spouses of a person
        private List<Person> GetSpousesOfPerson(Person person)
        {
            var spouses = new List<Person>();
            
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.wifeId).FirstOrDefault();
                if (spouse != null) spouses.Add(spouse);
            }
            
            marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, false);
            foreach (var marriage in marriages)
            {
                var spouse = _dataProvider.GetPerson(marriage.husbandId).FirstOrDefault();
                if (spouse != null) spouses.Add(spouse);
            }
            
            return spouses;
        }

        // Helper method to get children of a person
        private List<Person> GetChildrenOfPerson(Person person)
        {
            var children = new List<Person>();
            
            var marriages = _dataProvider.GetMarriages(person.dataBaseOwnerId, true);
            marriages.AddRange(_dataProvider.GetMarriages(person.dataBaseOwnerId, false));
            
            foreach (var marriage in marriages)
            {
                var marriageChildren = _dataProvider.GetChildren(marriage.familyId);
                foreach (var child in marriageChildren)
                {
                    var childPerson = _dataProvider.GetPerson(child.childId).FirstOrDefault();
                    if (childPerson != null && !children.Any(c => c.dataBaseOwnerId == childPerson.dataBaseOwnerId))
                    {
                        children.Add(childPerson);
                    }
                }
            }
            
            return children;
        }

        // Utility methods
        private string GetParentsNames(List<Person> parents)
        {
            if (parents.Count == 0) return "Unknown";
            if (parents.Count == 1) return FormatPersonName(parents[0]);
            
            var father = parents.FirstOrDefault(p => p.gender == PersonGenderType.Male);
            var mother = parents.FirstOrDefault(p => p.gender == PersonGenderType.Female);
            
            if (father != null && mother != null)
            {
                if (showDataBaseOwnerId)
                    return $"{father.givenName} ({father.dataBaseOwnerId}) and {mother.givenName} ({mother.dataBaseOwnerId}) {father.surName}";
                else
                    return $"{father.givenName} and {mother.givenName} {father.surName}";
            }
            
            return string.Join(" and ", parents.Select(p => FormatPersonName(p)));
        }

        private string GetGenderText(PersonGenderType gender)
        {
            return gender == PersonGenderType.Male ? "son" : (gender == PersonGenderType.Female ? "daughter" : "child");
        }

        private string FormatDate(int month, int day, int year)
        {
            if (year == 0) return "Unknown";
            if (month == 0 || day == 0) return year.ToString();
            
            return $"{GetMonthName(month)} {day}, {year}";
        }

        private string GetMonthName(int month)
        {
            string[] months = { "", "January", "February", "March", "April", "May", "June",
                               "July", "August", "September", "October", "November", "December" };
            return month >= 1 && month <= 12 ? months[month] : "";
        }

        private string CalculateAge(int birthYear, int currentYear)
        {
            if (birthYear == 0 || currentYear == 0) return "unknown age";
            int age = currentYear - birthYear;
            return age > 0 ? $"{age} years old" : "infant";
        }

        /// <summary>
        /// Public method to test DAG functionality - call this from Unity Inspector or other scripts
        /// </summary>
        public void TestDAGFunctionality()
        {
            Debug.Log($"[FamilyHappeningsContent] === MANUAL DAG TEST ===");
            
            if (_familyDAG == null)
            {
                Debug.LogError("[FamilyHappeningsContent] DAG is null! Cannot test functionality.");
                return;
            }
            
            if (_familyDAG.People.Count == 0)
            {
                Debug.LogWarning("[FamilyHappeningsContent] DAG has no people! Check if DAG was built properly.");
                return;
            }
            
            // Test with a sample person
            var testPerson = _familyDAG.People.Values.First();
            Debug.Log($"[FamilyHappeningsContent] Testing with person: {testPerson.givenName} {testPerson.surName} (ID: {testPerson.dataBaseOwnerId})");
            
            // Test family gathering
            var familyMembers = GetCloseFamilyMembers(testPerson);
            Debug.Log($"[FamilyHappeningsContent] Found {familyMembers.Count} family members for {testPerson.givenName}");
            
            // Test relationship determination with a few family members
            foreach (var familyMember in familyMembers.Take(3))
            {
                if (familyMember.dataBaseOwnerId != testPerson.dataBaseOwnerId)
                {
                    var relationship = GetRelationshipToPerson(familyMember, testPerson);
                    Debug.Log($"[FamilyHappeningsContent] {familyMember.givenName} is {testPerson.givenName}'s {relationship}");
                }
            }
            
            Debug.Log($"[FamilyHappeningsContent] === END MANUAL DAG TEST ===");
        }
        
        /// <summary>
        /// Get DAG statistics for debugging
        /// </summary>
        public string GetDAGStatistics()
        {
            if (_familyDAG == null)
                return "DAG is null";
                
            var stats = new System.Text.StringBuilder();
            stats.AppendLine($"DAG Statistics:");
            stats.AppendLine($"  Total People: {_familyDAG.People.Count}");
            
            int totalRelationships = 0;
            int peopleWithRelationships = 0;
            
            foreach (var person in _familyDAG.People.Values)
            {
                var relationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
                totalRelationships += relationships.Count;
                if (relationships.Count > 0)
                    peopleWithRelationships++;
            }
            
            stats.AppendLine($"  Total Relationships: {totalRelationships}");
            stats.AppendLine($"  People with Relationships: {peopleWithRelationships}");
            stats.AppendLine($"  Average Relationships per Person: {(peopleWithRelationships > 0 ? (double)totalRelationships / peopleWithRelationships : 0):F1}");
            
            return stats.ToString();
        }
    }
}
