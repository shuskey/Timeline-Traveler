using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.DataObjects
{
    /// <summary>
    /// Directed Acyclic Graph implementation for family relationships
    /// This provides efficient relationship traversal and pre-computed relationship paths
    /// </summary>
    public class FamilyDAG
    {
        private readonly Dictionary<int, Person> _people = new Dictionary<int, Person>();
        private readonly Dictionary<int, List<FamilyEdge>> _outgoingEdges = new Dictionary<int, List<FamilyEdge>>();
        private readonly Dictionary<int, List<FamilyEdge>> _incomingEdges = new Dictionary<int, List<FamilyEdge>>();
        private readonly Dictionary<(int, int), string> _relationshipCache = new Dictionary<(int, int), string>();

        public IReadOnlyDictionary<int, Person> People => _people;

        /// <summary>
        /// Add a person to the DAG
        /// </summary>
        public void AddPerson(Person person)
        {
            if (!_people.ContainsKey(person.dataBaseOwnerId))
            {
                _people[person.dataBaseOwnerId] = person;
                _outgoingEdges[person.dataBaseOwnerId] = new List<FamilyEdge>();
                _incomingEdges[person.dataBaseOwnerId] = new List<FamilyEdge>();
            }
        }

        /// <summary>
        /// Add a relationship edge between two people
        /// </summary>
        public void AddRelationship(int fromPersonId, int toPersonId, PersonRelationshipType relationshipType, 
            int? eventDate = null, ChildRelationshipType childRelationType = ChildRelationshipType.Biological)
        {
            if (!_people.ContainsKey(fromPersonId) || !_people.ContainsKey(toPersonId))
            {
                Debug.LogWarning($"[familyDAGDebugContent] Cannot add relationship: person {fromPersonId} or {toPersonId} not in DAG");
                return;
            }

            // Check for cycles before adding
            if (WouldCreateCycle(fromPersonId, toPersonId))
            {
                Debug.LogWarning($"[familyDAGDebugContent] Relationship {fromPersonId} -> {toPersonId} would create cycle, skipping");
                return;
            }

            var edge = new FamilyEdge
            {
                FromPersonId = fromPersonId,
                ToPersonId = toPersonId,
                RelationshipType = relationshipType,
                EventDate = eventDate,
                ChildRelationType = childRelationType
            };

            _outgoingEdges[fromPersonId].Add(edge);
            _incomingEdges[toPersonId].Add(edge);

            var fromPerson = _people[fromPersonId];
            var toPerson = _people[toPersonId];

            // Clear relationship cache as new edge may change relationships
            _relationshipCache.Clear();

            // Populate the Person's familyRelationships field
            if (fromPerson.familyRelationships == null)
                fromPerson.familyRelationships = new List<(PersonRelationshipType, Person)>();
            
            fromPerson.familyRelationships.Add((relationshipType, toPerson));
        }

        /// <summary>
        /// Get all direct relationships for a person
        /// </summary>
        public List<FamilyEdge> GetDirectRelationships(int personId)
        {
            var relationships = new List<FamilyEdge>();
            if (_outgoingEdges.ContainsKey(personId))
                relationships.AddRange(_outgoingEdges[personId]);
            if (_incomingEdges.ContainsKey(personId))
                relationships.AddRange(_incomingEdges[personId]);
            
            return relationships;
        }

        /// <summary>
        /// Get all ancestors of a person (parents, grandparents, etc.)
        /// </summary>
        public HashSet<Person> GetAncestors(int personId, int maxGenerations = 10)
        {
            if (!_people.ContainsKey(personId))
            {
                Debug.LogWarning($"[familyDAGDebugContent] Person {personId} not found in DAG");
                return new HashSet<Person>();
            }
            
            var ancestors = new HashSet<Person>();
            var visited = new HashSet<int>();
            GetAncestorsRecursive(personId, ancestors, visited, 0, maxGenerations);
            
            return ancestors;
        }

        private void GetAncestorsRecursive(int personId, HashSet<Person> ancestors, HashSet<int> visited, int currentLevel, int maxGenerations)
        {
            if (visited.Contains(personId) || currentLevel >= maxGenerations)
            {
                return;
            }

            visited.Add(personId);
            var person = _people[personId];

            if (_incomingEdges.ContainsKey(personId))
            {
                foreach (var edge in _incomingEdges[personId])
                {
                    // For ancestors, we want to find edges where this person is the "to" person
                    // and the relationship type indicates a parent relationship
                    if (edge.ToPersonId == personId && 
                        (edge.RelationshipType == PersonRelationshipType.Mother || edge.RelationshipType == PersonRelationshipType.Father))
                    {
                        var parent = _people[edge.FromPersonId];
                        ancestors.Add(parent);
                        GetAncestorsRecursive(edge.FromPersonId, ancestors, visited, currentLevel + 1, maxGenerations);
                    }
                }
            }
        }

        /// <summary>
        /// Get all descendants of a person (children, grandchildren, etc.)
        /// </summary>
        public HashSet<Person> GetDescendants(int personId, int maxGenerations = 10)
        {
            var descendants = new HashSet<Person>();
            var visited = new HashSet<int>();
            GetDescendantsRecursive(personId, descendants, visited, 0, maxGenerations);
            return descendants;
        }

        private void GetDescendantsRecursive(int personId, HashSet<Person> descendants, HashSet<int> visited, int currentLevel, int maxGenerations)
        {
            if (visited.Contains(personId) || currentLevel >= maxGenerations)
                return;

            visited.Add(personId);

            if (_outgoingEdges.ContainsKey(personId))
            {
                foreach (var edge in _outgoingEdges[personId])
                {
                    if (edge.RelationshipType == PersonRelationshipType.Child)
                    {
                        var child = _people[edge.ToPersonId];
                        descendants.Add(child);
                        GetDescendantsRecursive(edge.ToPersonId, descendants, visited, currentLevel + 1, maxGenerations);
                    }
                }
            }
        }

        /// <summary>
        /// Efficiently determine the relationship between any two people
        /// </summary>
        public string GetRelationshipBetween(int person1Id, int person2Id)
        {
            if (person1Id == person2Id)
                return "self";

            // Check cache first
            var cacheKey = (person1Id, person2Id);
            if (_relationshipCache.ContainsKey(cacheKey))
                return _relationshipCache[cacheKey];

            var relationship = CalculateRelationship(person1Id, person2Id);
            _relationshipCache[cacheKey] = relationship;
            return relationship;
        }

        private string CalculateRelationship(int person1Id, int person2Id)
        {
            // Check direct relationships first
            if (_outgoingEdges.ContainsKey(person1Id))
            {
                var directEdge = _outgoingEdges[person1Id].FirstOrDefault(e => e.ToPersonId == person2Id);
                if (directEdge != null)
                {
                    return GetDirectRelationshipString(directEdge);
                }
            }

            if (_incomingEdges.ContainsKey(person1Id))
            {
                var directEdge = _incomingEdges[person1Id].FirstOrDefault(e => e.FromPersonId == person2Id);
                if (directEdge != null)
                {
                    return GetInverseRelationshipString(directEdge);
                }
            }

            // Find path between persons using BFS
            var path = FindShortestPath(person1Id, person2Id);
            if (path != null && path.Count > 0)
            {
                return InterpretRelationshipPath(path, person1Id, person2Id);
            }

            return "relative"; // Default fallback
        }

        private List<FamilyEdge> FindShortestPath(int startPersonId, int endPersonId)
        {
            var queue = new Queue<(int personId, List<FamilyEdge> path)>();
            var visited = new HashSet<int>();
            
            queue.Enqueue((startPersonId, new List<FamilyEdge>()));
            visited.Add(startPersonId);

            while (queue.Count > 0)
            {
                var (currentPersonId, currentPath) = queue.Dequeue();

                // Check outgoing edges
                if (_outgoingEdges.ContainsKey(currentPersonId))
                {
                    foreach (var edge in _outgoingEdges[currentPersonId])
                    {
                        if (edge.ToPersonId == endPersonId)
                        {
                            var completePath = new List<FamilyEdge>(currentPath) { edge };
                            return completePath;
                        }

                        if (!visited.Contains(edge.ToPersonId) && currentPath.Count < 5) // Limit search depth
                        {
                            visited.Add(edge.ToPersonId);
                            var newPath = new List<FamilyEdge>(currentPath) { edge };
                            queue.Enqueue((edge.ToPersonId, newPath));
                        }
                    }
                }

                // Check incoming edges
                if (_incomingEdges.ContainsKey(currentPersonId))
                {
                    foreach (var edge in _incomingEdges[currentPersonId])
                    {
                        if (edge.FromPersonId == endPersonId)
                        {
                            var completePath = new List<FamilyEdge>(currentPath) { edge };
                            return completePath;
                        }

                        if (!visited.Contains(edge.FromPersonId) && currentPath.Count < 5) // Limit search depth
                        {
                            visited.Add(edge.FromPersonId);
                            var newPath = new List<FamilyEdge>(currentPath) { edge };
                            queue.Enqueue((edge.FromPersonId, newPath));
                        }
                    }
                }
            }

            return null; // No path found
        }

        private string InterpretRelationshipPath(List<FamilyEdge> path, int person1Id, int person2Id)
        {
            // Simple interpretation - this can be expanded for more complex relationships
            if (path.Count == 1)
            {
                return GetDirectRelationshipString(path[0]);
            }
            else if (path.Count == 2)
            {
                // Handle grandparent/grandchild relationships
                if (path.All(e => e.RelationshipType == PersonRelationshipType.Mother || e.RelationshipType == PersonRelationshipType.Father))
                {
                    // For grandparent relationships, we want the gender of the person being described (person1Id)
                    // person1Id is the grandparent whose relationship we're describing
                    var person = _people[person1Id];
                    return person.gender == PersonGenderType.Male ? "grandfather" : "grandmother";
                }
                if (path.All(e => e.RelationshipType == PersonRelationshipType.Child))
                {
                    // For grandchild relationships, we want the gender of the person being described (person1Id)
                    // person1Id is the one whose relationship we're describing
                    var person = _people[person1Id];
                    return person.gender == PersonGenderType.Male ? "grandson" : "granddaughter";
                }
                
                // Handle sibling relationships (should be 2 edges: person1 -> parent -> person2)
                if (path.Any(e => e.RelationshipType == PersonRelationshipType.Mother || e.RelationshipType == PersonRelationshipType.Father) &&
                    path.Any(e => e.RelationshipType == PersonRelationshipType.Child))
                {
                    var person = _people[person1Id];
                    return person.gender == PersonGenderType.Male ? "brother" : "sister";
                }
            }

            return $"{path.Count}-degree relative"; // Generic relationship for complex paths
        }

        private string GetDirectRelationshipString(FamilyEdge edge)
        {
            return edge.RelationshipType switch
            {
                PersonRelationshipType.Mother => "mother",
                PersonRelationshipType.Father => "father",
                PersonRelationshipType.Child => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "son" : "daughter",
                PersonRelationshipType.Spouse => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "husband" : "wife",
                PersonRelationshipType.Sibling => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "brother" : "sister",
                PersonRelationshipType.NieceNephew => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "nephew" : "niece",
                PersonRelationshipType.AuntUncle => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "uncle" : "aunt",
                PersonRelationshipType.Cousin => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "cousin" : "cousin",
                PersonRelationshipType.GrandParent => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "grandfather" : "grandmother",
                PersonRelationshipType.GrandChild => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "grandson" : "granddaughter",
                _ => "relative"
            };
        }

        private string GetInverseRelationshipString(FamilyEdge edge)
        {
            return edge.RelationshipType switch
            {
                PersonRelationshipType.Mother => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "son" : "daughter",
                PersonRelationshipType.Father => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "son" : "daughter",
                PersonRelationshipType.Child => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "son" : "daughter",
                PersonRelationshipType.Spouse => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "husband" : "wife",
                PersonRelationshipType.Sibling => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "brother" : "sister",
                PersonRelationshipType.NieceNephew => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "uncle" : "aunt",
                PersonRelationshipType.AuntUncle => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "nephew" : "niece",
                PersonRelationshipType.Cousin => _people[edge.ToPersonId].gender == PersonGenderType.Male ? "cousin" : "cousin",
                PersonRelationshipType.GrandParent => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "grandson" : "granddaughter",
                PersonRelationshipType.GrandChild => _people[edge.FromPersonId].gender == PersonGenderType.Male ? "grandfather" : "grandmother",
                _ => "relative"
            };
        }

        /// <summary>
        /// Check if adding an edge would create a cycle (violate DAG property)
        /// Allow bidirectional relationships but prevent true cycles
        /// </summary>
        private bool WouldCreateCycle(int fromPersonId, int toPersonId)
        {
            // Check if this would create a direct bidirectional relationship
            // This is allowed for family relationships (parent-child, spouse-spouse)
            if (_outgoingEdges.ContainsKey(toPersonId))
            {
                var existingEdge = _outgoingEdges[toPersonId].FirstOrDefault(e => e.ToPersonId == fromPersonId);
                if (existingEdge != null)
                {
                    return false; // Allow bidirectional relationships
                }
            }

            // Use DFS to see if we can reach fromPersonId starting from toPersonId
            // This prevents true cycles (A -> B -> C -> A)
            var visited = new HashSet<int>();
            return CanReach(toPersonId, fromPersonId, visited);
        }

        private bool CanReach(int startId, int targetId, HashSet<int> visited)
        {
            if (startId == targetId)
                return true;

            if (visited.Contains(startId))
                return false;

            visited.Add(startId);

            if (_outgoingEdges.ContainsKey(startId))
            {
                foreach (var edge in _outgoingEdges[startId])
                {
                    if (CanReach(edge.ToPersonId, targetId, visited))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Debug method to dump the entire DAG structure
        /// </summary>
        public void DumpDAGStructure(int startingPersonId)
        {
            Debug.Log($"Dumping DAG structure for starting person {startingPersonId}");
            // Log the starting person
            Debug.Log($"Starting person: {_people[startingPersonId].givenName} {_people[startingPersonId].surName}");
            
            foreach (var person in _people)
            {
                // Log the person's name and their relationship to the starting person
                Debug.Log($"Person {person.Key}: {person.Value.givenName} {person.Value.surName} - {GetRelationshipBetween(person.Key, startingPersonId)}");
            }
        }

        /// <summary>
        /// Get all people at a specific generation level relative to a root person
        /// </summary>
        public List<Person> GetPeopleAtGeneration(int rootPersonId, int generation)
        {
            var result = new List<Person>();
            var visited = new HashSet<int>();
            
            if (generation == 0)
            {
                if (_people.ContainsKey(rootPersonId))
                    result.Add(_people[rootPersonId]);
                return result;
            }

            GetPeopleAtGenerationRecursive(rootPersonId, generation, 0, result, visited);
            return result;
        }

        private void GetPeopleAtGenerationRecursive(int personId, int targetGeneration, int currentGeneration, 
            List<Person> result, HashSet<int> visited)
        {
            if (visited.Contains(personId))
                return;

            visited.Add(personId);

            if (currentGeneration == targetGeneration)
            {
                if (_people.ContainsKey(personId))
                    result.Add(_people[personId]);
                return;
            }

            // Navigate up (negative generation) or down (positive generation)
            var edgesToCheck = targetGeneration > currentGeneration ? _outgoingEdges : _incomingEdges;
            var relationshipTypes = targetGeneration > currentGeneration 
                ? new[] { PersonRelationshipType.Child }
                : new[] { PersonRelationshipType.Mother, PersonRelationshipType.Father };

            if (edgesToCheck.ContainsKey(personId))
            {
                foreach (var edge in edgesToCheck[personId])
                {
                    if (relationshipTypes.Contains(edge.RelationshipType))
                    {
                        var nextPersonId = targetGeneration > currentGeneration ? edge.ToPersonId : edge.FromPersonId;
                        var nextGeneration = targetGeneration > currentGeneration ? currentGeneration + 1 : currentGeneration - 1;
                        GetPeopleAtGenerationRecursive(nextPersonId, targetGeneration, nextGeneration, result, visited);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a directed edge in the family DAG
    /// </summary>
    public class FamilyEdge
    {
        public int FromPersonId { get; set; }
        public int ToPersonId { get; set; }
        public PersonRelationshipType RelationshipType { get; set; }
        public int? EventDate { get; set; } // Marriage date, birth date, etc.
        public ChildRelationshipType ChildRelationType { get; set; } = ChildRelationshipType.Biological;
    }
} 