using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using Assets.Scripts.DataObjects;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;
using Assets.Scripts.ServiceProviders;

/// <summary>
/// Example showing how to refactor the Tribe class to use FamilyDAG
/// This demonstrates the improved efficiency and capabilities of the DAG approach
/// </summary>
public class TribeDAGExample : MonoBehaviour
{
    [Header("DAG Configuration")]
    [SerializeField]
    [Tooltip("Number of generations to load in each direction")]
    private int generationsToLoad = 3;

    [SerializeField]
    [Tooltip("Starting person ID for the family tree")]
    private int startingPersonId = 1;

    private FamilyDAG _familyDAG;
    private FamilyDAGBuilder _dagBuilder;
    private IFamilyHistoryDataProvider _dataProvider;
    
    // For backward compatibility during migration
    private Dictionary<int, List<Person>> _generationView;

    void Start()
    {
        StartCoroutine(InitializeDAGAsync());
    }

    /// <summary>
    /// Initialize the DAG - this replaces the complex generation-based loading in the original Tribe
    /// </summary>
    private IEnumerator InitializeDAGAsync()
    {
        Debug.Log("Initializing Family DAG...");
        
        // Initialize data provider (same as before)
        _dataProvider = new RootsMagicFamilyHistoryDataProvider();
        var config = new Dictionary<string, string>
        {
            			{ PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, 
              CrossSceneInformation.rootsMagicDataFileNameWithFullPath }
        };
        _dataProvider.Initialize(config);

        // Build the DAG - this is MUCH simpler than the original approach
        _dagBuilder = new FamilyDAGBuilder(_dataProvider);
        _familyDAG = _dagBuilder.BuildDAGForPerson(startingPersonId, generationsToLoad, generationsToLoad);

        Debug.Log($"DAG initialization complete! Loaded {_familyDAG.People.Count} people.");

        // For backward compatibility, create generation view
        _generationView = _dagBuilder.GetPeopleByGeneration(startingPersonId);

        // Create game objects (much simpler now)
        CreatePersonGameObjects();

        yield return null;
    }

    /// <summary>
    /// Create game objects - simplified because relationships are already established
    /// </summary>
    private void CreatePersonGameObjects()
    {
        foreach (var person in _familyDAG.People.Values)
        {
            CreatePersonGameObject(person);
        }

        // Visual connections are now trivial to create
        CreateVisualConnections();
    }

    /// <summary>
    /// Create visual connections between related people - much simpler with DAG
    /// </summary>
    private void CreateVisualConnections()
    {
        foreach (var person in _familyDAG.People.Values)
        {
            var relationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
            
            foreach (var relationship in relationships)
            {
                CreateVisualConnection(relationship);
            }
        }
    }

    private void CreateVisualConnection(FamilyEdge relationship)
    {
        var fromPerson = _familyDAG.People[relationship.FromPersonId];
        var toPerson = _familyDAG.People[relationship.ToPersonId];

        // Simple connection logic based on relationship type
        switch (relationship.RelationshipType)
        {
            case PersonRelationshipType.Spouse:
                CreateMarriageConnection(fromPerson, toPerson, relationship.EventDate ?? 0);
                break;
            case PersonRelationshipType.Child:
                CreateParentChildConnection(fromPerson, toPerson);
                break;
            // Add other visual connections as needed
        }
    }

    /// <summary>
    /// Demonstrate the power of DAG: instant relationship queries
    /// This replaces the complex recursive algorithms in FamilyHappeningsContent
    /// </summary>
    public void DemonstrateRelationshipQueries()
    {
        var people = _familyDAG.People.Values.Take(2).ToList();
        if (people.Count >= 2)
        {
            var person1 = people[0];
            var person2 = people[1];
            
            // Instant relationship determination - no expensive database queries!
            var relationship = _familyDAG.GetRelationshipBetween(person1.dataBaseOwnerId, person2.dataBaseOwnerId);
            Debug.Log($"Relationship between {person1.givenName} and {person2.givenName}: {relationship}");
            
            // Instant ancestor queries
            var ancestors = _familyDAG.GetAncestors(person1.dataBaseOwnerId);
            Debug.Log($"{person1.givenName} has {ancestors.Count} ancestors");
            
            // Instant descendant queries
            var descendants = _familyDAG.GetDescendants(person1.dataBaseOwnerId);
            Debug.Log($"{person1.givenName} has {descendants.Count} descendants");
        }
    }

    /// <summary>
    /// Dynamic expansion - much simpler with DAG
    /// This replaces LoadNextLevelOfDescendancyForPerson and LoadNextLevelOfAncestryForPerson
    /// </summary>
    public void ExpandFamilyTreeForPerson(int personId, int additionalGenerations = 2)
    {
        Debug.Log($"Dynamically expanding tree for person {personId}");
        
        // Build additional DAG data
        var expandedDAG = _dagBuilder.BuildDAGForPerson(personId, additionalGenerations, additionalGenerations);
        
        // The DAG automatically handles merging and deduplication
        Debug.Log($"Tree expanded! Now have {expandedDAG.People.Count} people total.");
        
        // Create game objects for new people
        foreach (var person in expandedDAG.People.Values)
        {
            if (!_familyDAG.People.ContainsKey(person.dataBaseOwnerId))
            {
                CreatePersonGameObject(person);
            }
        }
        
        // Update our main DAG reference
        _familyDAG = expandedDAG;
    }

    // Simplified helper methods
    private GameObject CreatePersonGameObject(Person person)
    {
        // Much simpler than the original - just create the visual representation
        var position = CalculatePersonPosition(person);
        var personGO = Instantiate(GetPersonPrefab(), position, Quaternion.identity);
        personGO.name = $"{person.givenName} {person.surName}";
        
        var personNode = personGO.GetComponent<PersonNode>();
        if (personNode != null)
        {
            personNode.SetIndexes(person.dataBaseOwnerId, person.tribeArrayIndex, person);
            // Other setup...
        }
        
        person.personNodeGameObject = personGO;
        return personGO;
    }

    private Vector3 CalculatePersonPosition(Person person)
    {
        // Use the generation from DAG or calculate based on relationships
        var x = person.indexIntoPersonsInThisGeneration * 20; // personSpacing
        var y = person.generation * 30; // generationGap
        var z = person.birthEventDate * 5; // time dimension
        
        return new Vector3(x, y, z);
    }

    private void CreateMarriageConnection(Person husband, Person wife, int marriageYear)
    {
        // Create visual marriage connection
        Debug.Log($"Creating marriage connection between {husband.givenName} and {wife.givenName} in {marriageYear}");
    }

    private void CreateParentChildConnection(Person parent, Person child)
    {
        // Create visual parent-child connection
        Debug.Log($"Creating parent-child connection: {parent.givenName} -> {child.givenName}");
    }

    private GameObject GetPersonPrefab()
    {
        // Return your person prefab
        return Resources.Load<GameObject>("PersonPrefab");
    }

    /// <summary>
    /// Demonstrate advanced DAG capabilities not possible with the old approach
    /// </summary>
    public void DemonstrateAdvancedCapabilities()
    {
        Debug.Log("=== Advanced DAG Capabilities Demo ===");
        
        // Find all people with a specific relationship type
        var allSpouses = new Dictionary<Person, List<Person>>();
        foreach (var person in _familyDAG.People.Values)
        {
            var relationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
            var spouses = relationships
                .Where(r => r.RelationshipType == PersonRelationshipType.Spouse && r.FromPersonId == person.dataBaseOwnerId)
                .Select(r => _familyDAG.People[r.ToPersonId])
                .ToList();
            
            if (spouses.Count > 0)
                allSpouses[person] = spouses;
        }
        Debug.Log($"Found {allSpouses.Count} people with spouses");
        
        // Find complex relationship patterns (e.g., people who are both cousins and in-laws)
        var complexRelationships = new List<string>();
        var people = _familyDAG.People.Values.ToList();
        
        for (int i = 0; i < people.Count && i < 10; i++) // Limit for demo
        {
            for (int j = i + 1; j < people.Count && j < 10; j++)
            {
                var relationship = _familyDAG.GetRelationshipBetween(people[i].dataBaseOwnerId, people[j].dataBaseOwnerId);
                if (relationship.Contains("cousin") || relationship.Contains("in-law"))
                {
                    complexRelationships.Add($"{people[i].givenName} and {people[j].givenName}: {relationship}");
                }
            }
        }
        
        Debug.Log($"Complex relationships found: {string.Join(", ", complexRelationships)}");
    }

    /// <summary>
    /// Backward compatibility method - get people by generation (for gradual migration)
    /// </summary>
    public Dictionary<int, List<Person>> GetPeopleByGeneration()
    {
        return _generationView ?? new Dictionary<int, List<Person>>();
    }
}

/// <summary>
/// Comparison of old vs new approach
/// </summary>
public static class DAGComparisonExample
{
    /// <summary>
    /// OLD APPROACH: Complex, database-heavy, error-prone
    /// </summary>
    public static void OldWayToFindRelationship(IFamilyHistoryDataProvider dataProvider, int person1Id, int person2Id)
    {
        // This is what you currently do in FamilyHappeningsContent.cs:
        // 1. Multiple database calls
        // 2. Complex recursive algorithms
        // 3. Circular reference detection needed
        // 4. No caching of results
        // 5. Lots of code for each relationship type
        
        Debug.Log("Old way: Multiple database queries, complex recursion, no caching...");
    }
    
    /// <summary>
    /// NEW APPROACH: Simple, fast, reliable
    /// </summary>
    public static void NewWayToFindRelationship(FamilyDAG familyDAG, int person1Id, int person2Id)
    {
        // With DAG: One simple method call, with caching!
        var relationship = familyDAG.GetRelationshipBetween(person1Id, person2Id);
        Debug.Log($"New way: {relationship} (cached result, no database calls needed!)");
    }
} 