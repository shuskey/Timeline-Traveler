using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine.SceneManagement;

public class PersonNode : MonoBehaviour
{
    public Person person;
    private PersonDetailsHandler personDetailsHandlerScript;
    private GlobalSpringType globalSpringType;
    public float lifeSpan;
    public int birthDate;
    public int endOfPlatformDate;
    public bool isLiving;
    public PersonGenderType personGender;
    public int dataBaseOwnerID;
    public int arrayIndex;
    public (int original, int updated) birthDateQuality;
    public (int original, int updated) deathDateQuality;
    public string dateQualityInformationString = "";
    public int GenerationDepth => person?.generation ?? 0;

    private bool debugAddMotion = false;
    private Rigidbody myRigidbody;

    GameObject birthConnectionPrefabObject;
    GameObject marriageConnectionPrefabObject;
    GameObject hallOfHistoryGameObject;
    GameObject hallOfFamilyPhotosGameObject;
    GameObject bubblePrefabObject;
    GameObject parentPlatformBirthBubble;
    GameObject childPlatformReturnToParent;
    GameObject parentBirthConnectionPoint;
    GameObject myMarriageConnectionPoint;
    GameObject spouseMarriageConnectionPoint;
    GameObject returnToMotherBirthConnectionPoint;
    GameObject returnToFatherBirthConnectionPoint;
    GameObject childBirthConnectionPoint;

    const int ScaleThisChildIndex = 0;
    static Color clearWhite = new Color(1.0f, 1.0f, 1.0f, 0.2f);
    static Color pink = new Color(0.8f, 0.5f, 0.8f, 0.7f);
    static Color blue = new Color(0.4f, 0.7f, 0.9f, 0.7f);

    private Color[] personGenderCapsuleBubbleColors = {
        clearWhite,   // notset
        blue,   // male
        pink    // female
        };
    private Color[] personDateQualityColors = {
        new Color(0.4f, 0.4f, 0.4f, 0.5f),   // date = orig
        new Color(0.9f, 0.1f, 0.1f, 0.5f)    // date != orig
        };

    private Color[] personGenderPlatformColors = {
        new Color(0.2f, 0.2f, 0.2f),   // notset
        new Color(0.3f, 0.6f, 0.9f),   // male
        new Color(0.7f, 0.4f, 0.9f)    // female
        };
    private Color[] livingOrNotPlatformColors = {
         new Color(0.7903f, 0.8018f, 0.7829f),   // not living
         new Color(0.6877f, 0.9056f, 0.8028f)   // living
           
    };
    private Color[] childRelationshipColors = {
        new Color(0.9f, 0.9f, 0.9f, 0.3f),   // biological
        new Color(0.0f, 0.0f, 0.0f, 0.3f)    // adopted
        };

    void Start()
    {        
        var bothTextMeshProItems = transform.GetComponentsInChildren<TextMeshPro>();
        foreach (var textMeshProItem in bothTextMeshProItems)
        {
            if (textMeshProItem.tag.Contains("PersonName"))
                textMeshProItem.text = name;
        }
        GameObject[] personDetailsPanel = GameObject.FindGameObjectsWithTag("PersonDetailsPanel");
     
        personDetailsHandlerScript = personDetailsPanel[0].transform.GetComponent<PersonDetailsHandler>();
    }

    void Update()
    {
        if (debugAddMotion)
        {
            myRigidbody = this.transform.GetComponent<Rigidbody>();
            var vVelocityDirection = new Vector3(-1, 0, 0);
            myRigidbody.linearVelocity = vVelocityDirection * 1f;
        }
    }

    public void UpdatePersonDetailsWithThisPerson(int currentDate)
    {
        personDetailsHandlerScript.DisplayThisPerson(person, currentDate);

        StartCoroutine(hallOfFamilyPhotosGameObject.GetComponent<HallOfFamilyPhotos>().SetFocusPersonNode(this));
        if (hallOfHistoryGameObject != null)  // Game option set in tribe scene 
        StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(this));

        // Request loading next level of descendancy
        var tribe = GetComponentInParent<Tribe>();
        if (tribe != null)
        {
            tribe.LoadNextLevelOfDescendancyForPerson(dataBaseOwnerID, GenerationDepth, person.gender);
            tribe.LoadNextLevelOfAncestryForPerson(dataBaseOwnerID, GenerationDepth);
        }
    }

    public void ClearPersonDetails()
    {
        personDetailsHandlerScript.DisplayThisPerson(null);
    }

    public void SetEdgePrefab(GameObject birthConnectionPrefab, GameObject marriageConnectionPrefab, GameObject bubble, GameObject parentPlatformBirthBubble, GameObject childPlatformReturnToParent)
    {
        this.birthConnectionPrefabObject = birthConnectionPrefab;
        this.marriageConnectionPrefabObject = marriageConnectionPrefab;
        this.bubblePrefabObject = bubble;
        this.parentPlatformBirthBubble = parentPlatformBirthBubble;
        this.childPlatformReturnToParent = childPlatformReturnToParent;
    }

    public void SetHallOfHistoryGameObject(GameObject hallOfHistory)
    {
        this.hallOfHistoryGameObject = hallOfHistory;
        
        // Establish physical parent-child relationship so hall of history moves with PersonNode
        if (hallOfHistory != null)
        {
            hallOfHistory.transform.SetParent(this.transform, false);
            // Position it relative to the PersonNode (offset from family photos to avoid overlap)
            hallOfHistory.transform.localPosition = new Vector3(3f, 4f, 0); // To the right and above the person
            hallOfHistory.transform.localRotation = Quaternion.identity;
        }
    }

    public void SetHallOfFamilyPhotosGameObject(GameObject hallOfFamilyPhotos)
    {
        this.hallOfFamilyPhotosGameObject = hallOfFamilyPhotos;
    }

    public void SetGlobalSpringType(GlobalSpringType globalSpringType)
    {
        this.globalSpringType = globalSpringType;
        if (this.globalSpringType == GlobalSpringType.Crazy)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            return;
        }
        if (this.globalSpringType == GlobalSpringType.Normal)
        {
            this.transform.GetComponent<Rigidbody>().constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotation;
            return;
        }
        if (this.globalSpringType == GlobalSpringType.Freeze)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            this.transform.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void SetIndexes(int dataBaseOwnerId, int arrayIndex, Person person)
    {
        this.dataBaseOwnerID = dataBaseOwnerId;
        this.arrayIndex = arrayIndex;
        this.person = person;
    }

    public void SetLifeSpan(int birthDate, float age, bool isLiving)
    {
        var myScaleThisPlatformComponent = gameObject.transform.GetChild(ScaleThisChildIndex);
        // We want to scale the platform to the age of the person, with a minimum length of 5
        // Add 1 to include the current year (age represents completed years, but we want to include the current year)
        // Subtract 0.5 to prevent walking to the very end from triggering the next year
        float platformLength = (age + 1) * 5; // FOR NOW remove- 0.5f;
        myScaleThisPlatformComponent.transform.localScale = new Vector3(1.0f, 1.0f, Mathf.Max(5.0f, platformLength));
        //myPlatformComponent.transform.localPosition = new Vector3(0, 0, age / 2f);
        lifeSpan = age;
        this.birthDate = birthDate;
        this.endOfPlatformDate = birthDate + (int)age;
        this.isLiving = isLiving;

        var rendererParent = gameObject.GetComponentInChildren<Renderer>();
        var platforms = rendererParent.GetComponentsInChildren<Renderer>();
        // We want to color the platform based on whether the person is living or not
        // We also need to fix up the Texture Scale to match the platform size
        foreach (var renderer in platforms)
        {  
            renderer.material.mainTextureScale = new Vector2( renderer.bounds.size.x,  renderer.bounds.size.z);
            renderer.material.SetColor("_BaseColor", livingOrNotPlatformColors[isLiving ? 1 : 0]);
        }
            
    }

    public void AddDateQualityInformation((int updated, int original) birthDateQuality, (int updated, int original) deathDateQuality, string dateQualityInformationString)
    {
        this.dateQualityInformationString = dateQualityInformationString;
        this.birthDateQuality = birthDateQuality;
        this.deathDateQuality = deathDateQuality;
    }

    public void addMyBirthQualityBubble()
    {
        var myScaleThisPlatformTransform = gameObject.transform.GetChild(ScaleThisChildIndex);

        var birthConnection = Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorToSet = personDateQualityColors[birthDateQuality.original == birthDateQuality.updated ? 0 : 1];
        birthConnection.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", colorToSet);

        birthConnection.transform.localScale = Vector3.one * 1.5f;
        birthConnection.transform.parent = myScaleThisPlatformTransform;
        birthConnection.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void SetPersonGender(PersonGenderType personGender)
    {
        this.personGender = personGender;
        var renderer = gameObject.GetComponentsInChildren<Renderer>().Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        // set the color of the renderer
        renderer.material.SetColor("_BaseColor", personGenderPlatformColors[(int)personGender]);
    }

    /// <summary>
    /// Sets up the physics connection between parent and child platforms using SpringJoint
    /// </summary>
    private void SetupPhysicsConnection(PersonNode childPersonNode, float myAgeConnectionPointPercent)
    {
        var childAgeConnectionPointPercent = 0f;
        var parentRidgidbodyComponent = gameObject.transform.GetComponent<Rigidbody>();
        var childRidgidbodyComponent = childPersonNode.transform.GetComponent<Rigidbody>();
        
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = parentRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);
            sj.connectedAnchor = new Vector3(0, 0.5f, childAgeConnectionPointPercent);
            sj.enableCollision = true;
            sj.connectedBody = childRidgidbodyComponent;
            sj.spring = 0.01f;
        }
    }

    /// <summary>
    /// Creates and configures the parent connection point (birth bubble)
    /// </summary>
    private void CreateParentConnectionPoint(PersonNode childPersonNode, float myAgeConnectionPointPercent)
    {
        parentBirthConnectionPoint = Instantiate(this.parentPlatformBirthBubble, Vector3.zero, Quaternion.identity);
        
        // Set visual appearance based on child's gender
        var renderer = parentBirthConnectionPoint.GetComponentInChildren<Renderer>();
        renderer.material.SetColor("_BaseColor", personGenderCapsuleBubbleColors[(int)childPersonNode.personGender]);

        // Position the bubble on the parent platform
        parentBirthConnectionPoint.transform.parent = gameObject.transform.GetChild(0);  // Point to the ScaleThis Section
        parentBirthConnectionPoint.transform.localPosition = new Vector3(0, 0.5f, myAgeConnectionPointPercent);

        // Set up child name display
        var textMeshProItem = parentBirthConnectionPoint.transform.GetComponentsInChildren<TextMeshPro>().First();
        if (textMeshProItem.tag.Contains("ChildName"))
            textMeshProItem.text = childPersonNode.person.givenName.Split(' ')[0];

        // Set up teleportation to child
        var triggerTeleportToChildScript = parentBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
        triggerTeleportToChildScript.teleportTargetChild = childPersonNode.transform;
        triggerTeleportToChildScript.teleportOffset = new Vector3(0, 2.5f, 0);
        triggerTeleportToChildScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
        triggerTeleportToChildScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;
    }

    /// <summary>
    /// Creates and configures the child connection point
    /// </summary>
    private void CreateChildConnectionPoint(PersonNode childPersonNode)
    {
        var childAgeConnectionPointPercent = 0f;
        
        childBirthConnectionPoint = Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);
        var renderer = childBirthConnectionPoint.GetComponentInChildren<Renderer>();
        renderer.material.SetColor("_BaseColor", clearWhite);
        childBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        childBirthConnectionPoint.transform.parent = childPersonNode.transform.GetChild(0); // Point to the ScaleThis Section
        childBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, childAgeConnectionPointPercent);
    }

    /// <summary>
    /// Sets up return navigation from child back to parent
    /// </summary>
    private void SetupReturnNavigation(PersonNode childPersonNode, float myAgeConnectionPointPercent)
    {
        if (this.personGender == PersonGenderType.Male)
        {
            returnToFatherBirthConnectionPoint = Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToFatherRenderer = returnToFatherBirthConnectionPoint.GetComponentInChildren<Renderer>(); 
            returnToFatherRenderer.material.SetColor("_BaseColor", blue);
            var triggerTeleportToFatherScript = returnToFatherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToFatherScript.teleportTargetChild = gameObject.transform;
            triggerTeleportToFatherScript.teleportOffset = new Vector3(-3f, 2.5f, myAgeConnectionPointPercent * this.lifeSpan * 5);
            triggerTeleportToFatherScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
            triggerTeleportToFatherScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;

            returnToFatherBirthConnectionPoint.transform.parent = childPersonNode.transform.GetChild(0); // Point to the ScaleThis Section
            returnToFatherBirthConnectionPoint.transform.localPosition = new Vector3(-3f, 0, 0);
        }
        else if (this.personGender == PersonGenderType.Female)
        {
            returnToMotherBirthConnectionPoint = Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToMotherRenderer = returnToMotherBirthConnectionPoint.GetComponentInChildren<Renderer>();
            returnToMotherRenderer.material.SetColor("_BaseColor", pink);
            var triggerTeleportToMotherScript = returnToMotherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToMotherScript.teleportTargetChild = gameObject.transform;
            triggerTeleportToMotherScript.teleportOffset = new Vector3(3f, 2.5f, myAgeConnectionPointPercent * this.lifeSpan * 5);
            triggerTeleportToMotherScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
            triggerTeleportToMotherScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;

            returnToMotherBirthConnectionPoint.transform.parent = childPersonNode.transform.GetChild(0); // Point to the ScaleThis Section
            returnToMotherBirthConnectionPoint.transform.localPosition = new Vector3(3f, 0, 0);
        }
    }

    /// <summary>
    /// Creates the visual edge connection between parent and child
    /// </summary>
    private void CreateChildVisualEdge(PersonNode childPersonNode, ChildRelationshipType childRelationshipType, int birthDate)
    {
        GameObject edge = Instantiate(this.birthConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Edge Connector forBirth {birthDate} {childPersonNode.name}";

        // Use coroutine to handle edge creation (avoiding null reference issues)
        StartCoroutine(CreateBirthEdge(edge, parentBirthConnectionPoint, childBirthConnectionPoint, personGenderPlatformColors[(int)childPersonNode.personGender]));

        // Set edge color based on relationship type
        edge.transform.GetChild(ScaleThisChildIndex).GetComponent<Renderer>().material.SetColor("_BaseColor",
            childRelationshipColors[(int)childRelationshipType]);
    }
    
    /// <summary>
    /// Creates the visual marriage edge connection between spouses
    /// </summary>
    private void CreateMarriageVisualEdge(PersonNode spousePersonNode, int marriageEventDate, int marriageLength)
    {
        GameObject edge = Instantiate(this.marriageConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Edge Connector for Marriage {marriageEventDate}, to {spousePersonNode.name}, duration {marriageLength}.";
                
        // add logic so that I can do a conditional breakpoint for debugging only if the edge name is "Birth 1956 Arabella Kennedy"
        if (this.person.dataBaseOwnerId == 16)
        {
            Debug.Log("CreateMarriageVisualEdge called for edge: " + edge.name);
        }
    
        // Use coroutine to handle edge creation (avoiding null reference issues)
        StartCoroutine(CreateMarriageEdge(edge, myMarriageConnectionPoint, spouseMarriageConnectionPoint, marriageLength));
    }

    public void AddBirthEdge(PersonNode childPersonNode, float myAgeConnectionPointPercent = 0f,
        ChildRelationshipType childRelationshipType = ChildRelationshipType.Biological, int birthDate = 0)
    {
        // Set up physics connection between platforms
        SetupPhysicsConnection(childPersonNode, myAgeConnectionPointPercent);
        
        // Create parent connection point (birth bubble)
        CreateParentConnectionPoint(childPersonNode, myAgeConnectionPointPercent);
        
        // Create child connection point
        CreateChildConnectionPoint(childPersonNode);
        
        // Set up return navigation from child to parent
        SetupReturnNavigation(childPersonNode, myAgeConnectionPointPercent);
        
        // Create the visual edge connection
        CreateChildVisualEdge(childPersonNode, childRelationshipType, birthDate);
    }

    /// <summary>
    /// Sets up the physics connection between spouse platforms using SpringJoint
    /// </summary>
    private void SetupMarriagePhysicsConnection(PersonNode spousePersonNode, float myAgeConnectionPointPercent, float spouseAgeConnectionPointPercent)
    {
        var myRidgidbodyComponent = gameObject.transform.GetComponent<Rigidbody>();
        var spouseRidgidbodyComponent = spousePersonNode.transform.GetComponent<Rigidbody>();
        
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);
            sj.connectedAnchor = new Vector3(0, 0.5f, spouseAgeConnectionPointPercent);
            sj.enableCollision = true;
            sj.connectedBody = spouseRidgidbodyComponent;
            sj.spring = 5f;
            sj.minDistance = 20.0f;
            sj.maxDistance = 80.0f;
        }
    }

    /// <summary>
    /// Creates connection points for both spouses in the marriage
    /// </summary>
    private void CreateMarriageConnectionPoints(PersonNode spousePersonNode, int marriageEventDate)
    {
        // Calculate world Z position for the marriage date
        float marriageWorldZ = marriageEventDate * 5f;
        
        // Calculate local Z positions relative to each person's platform
        float myLocalZ = (marriageWorldZ - this.birthDate * 5f) / this.transform.GetChild(0).localScale.z;
        float spouseLocalZ = (marriageWorldZ - spousePersonNode.birthDate * 5f) / spousePersonNode.transform.GetChild(0).localScale.z;
        
        // Create connection point for this person (spouse 1)
        myMarriageConnectionPoint = new GameObject();
        myMarriageConnectionPoint.transform.localScale = Vector3.one * 2f;
        myMarriageConnectionPoint.transform.parent = gameObject.transform.GetChild(0);  // Point to the ScaleThis Section
        myMarriageConnectionPoint.transform.localPosition = new Vector3(0, 0, myLocalZ);

        // Create connection point for spouse (spouse 2)
        spouseMarriageConnectionPoint = new GameObject();
        spouseMarriageConnectionPoint.transform.localScale = Vector3.one * 2f;
        spouseMarriageConnectionPoint.transform.parent = spousePersonNode.transform.GetChild(0);  // Point to the ScaleThis Section
        spouseMarriageConnectionPoint.transform.localPosition = new Vector3(0, 0, spouseLocalZ);

        Debug.Log($"Marriage Z Debug - Marriage Date: {marriageEventDate}, World Z: {marriageWorldZ}");
        Debug.Log($"My Local Z: {myLocalZ}, My World Z: {myMarriageConnectionPoint.transform.position.z}");
        Debug.Log($"Spouse Local Z: {spouseLocalZ}, Spouse World Z: {spouseMarriageConnectionPoint.transform.position.z}");
    }


    public void AddMarriageEdge(PersonNode spousePersonNode,
        float myAgeConnectionPointPercent = 0f,
        float spouseAgeConnectionPointPercent = 0f, int marriageEventDate = 0, int marriageLength = 0)
    {
        // Set up physics connection between spouse platforms
        SetupMarriagePhysicsConnection(spousePersonNode, myAgeConnectionPointPercent, spouseAgeConnectionPointPercent);
        
        // Create connection points for both spouses using the marriage date
        CreateMarriageConnectionPoints(spousePersonNode, marriageEventDate);
        
        // Create the visual marriage edge connection
        CreateMarriageVisualEdge(spousePersonNode, marriageEventDate, marriageLength);
    }

    /// <summary>
    /// Creates and configures a birth edge connection between parent and child
    /// </summary>
    private IEnumerator CreateBirthEdge(GameObject edge, GameObject parentBirthConnectionPoint, GameObject childBirthConnectionPoint, Color genderColor)
    {
        yield return new WaitForSeconds(0.5f);  // Wait longer for edge to be fully instantiated
        
        var edgeComponent = edge?.GetComponent<Edge>();
        if (edgeComponent == null || parentBirthConnectionPoint == null || childBirthConnectionPoint == null)
        {
            Debug.LogError("Null reference in CreateBirthEdge - skipping edge creation");
            yield break;
        }

        edgeComponent.CreateEdge(parentBirthConnectionPoint, childBirthConnectionPoint);
        
        var childRenderer = edge.transform.childCount > 0 ? edge.transform.GetChild(0)?.GetComponent<Renderer>() : null;
        if (childRenderer != null)
        {
            childRenderer.material.SetColor("_BaseColor", genderColor);
        }

        //edge.transform.parent = myPositionThisPlatformTransform;
    }

    /// <summary>
    /// Creates and configures a marriage edge connection between spouses
    /// </summary>
    private IEnumerator CreateMarriageEdge(GameObject edge, GameObject myMarriegeConnectionPoint, GameObject spouseMarriageConnectionPoint, int marriageLength)
    {
        yield return new WaitForSeconds(0.5f);  // Wait longer for edge to be fully instantiated
        
        var edgeComponent = edge?.GetComponent<Edge>();
        if (edgeComponent == null || myMarriegeConnectionPoint == null || spouseMarriageConnectionPoint == null)
        {
            Debug.LogError("Null reference in CreateMarriageEdge - skipping edge creation");
            yield break;
        }
     
        edgeComponent.CreateEdge(myMarriegeConnectionPoint, spouseMarriageConnectionPoint);
        
        // Set edge length for marriage duration
        edgeComponent.SetEdgeEventLength(marriageLength * 5);
        
        var childRenderer = edge.transform.childCount > 0 ? edge.transform.GetChild(0)?.GetComponent<Renderer>() : null;
        if (childRenderer != null)
        {
            childRenderer.material.SetColor("_BaseColor", new Color(1.0f, 0.92f, 0.01f, 0.6f));
        }

        //edge.transform.parent = myPositionThisPlatformTransform;
    }

    public void SetDebugAddMotionSetting(bool newDebugAddMotionSetting)
    {
        this.debugAddMotion = newDebugAddMotionSetting;
    }
}