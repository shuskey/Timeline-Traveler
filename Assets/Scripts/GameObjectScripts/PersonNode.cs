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
    private Person person;
    private PersonDetailsHandler personDetailsHandlerScript;
    private GlobalSpringType globalSpringType;
    private string rootsMagicFileName;
    private string digiKamFileName;
    //private PrimaryPhotoForPersonRM primaryPhotoForPersonRM;

    
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

    float marriageConnectionXScale;
    GameObject bubblePrefabObject;
    GameObject parentPlatformBirthBubble;
    GameObject childPlatformReturnToParent;
    GameObject parentBirthConnectionPoint;
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

    public void SetEdgePrefab(GameObject birthConnectionPrefab, GameObject marriageConnectionPrefab, GameObject bubble, GameObject parentPlatformBirthBubble, GameObject childPlatformReturnToParent, float edgeXScale)
    {
        this.birthConnectionPrefabObject = birthConnectionPrefab;
        this.marriageConnectionPrefabObject = marriageConnectionPrefab;
        this.marriageConnectionXScale = edgeXScale;
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
        
        // Establish physical parent-child relationship so hall of family photos moves with PersonNode
        if (hallOfFamilyPhotos != null)
        {
            hallOfFamilyPhotos.transform.SetParent(this.transform, false);
            // Position it relative to the PersonNode (adjust these values as needed for your scene)
            hallOfFamilyPhotos.transform.localPosition = new Vector3(0, 5f, 0); // Above the person
            hallOfFamilyPhotos.transform.localRotation = Quaternion.identity;
        }
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
        myScaleThisPlatformComponent.transform.localScale = new Vector3(1.0f, 1.0f, Mathf.Max(5.0f, age * 5));
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

    public void AddBirthEdge(PersonNode childPersonNode, float myAgeConnectionPointPercent = 0f,
        ChildRelationshipType childRelationshipType = ChildRelationshipType.Biological, int birthDate = 0)
    {
        var foo = new Color(0.3f, 0.4f, 0.6f);
        var childAgeConnectionPointPercent = 0f;
        var parentPlatformTransform = gameObject.transform;
        var parentRidgidbodyComponent = parentPlatformTransform.transform.GetComponent<Rigidbody>();
        var childPlatformTransform = childPersonNode.transform;
        var childRidgidbodyComponent = childPlatformTransform.transform.GetComponent<Rigidbody>();
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = parentRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);
            sj.connectedAnchor = new Vector3(0, 0.5f, childAgeConnectionPointPercent);
            sj.enableCollision = true;
            sj.connectedBody = childRidgidbodyComponent;
            sj.spring = 0.01f; // 10.0f;
                               //sj.minDistance = 10.0f;
                               //sj.maxDistance = 500.0f;
        }
        parentBirthConnectionPoint = 
            Instantiate(this.parentPlatformBirthBubble, Vector3.zero, Quaternion.identity);
        //TODO Twins born at the same time are not handled well if one is a boy and the other a girl
        var renderer = parentBirthConnectionPoint.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_BaseColor",
            personGenderCapsuleBubbleColors[(int)childPersonNode.personGender]);

        //parentBirthConnectionPoint.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        //leftConnection.transform.localScale = Vector3.one * 2f;
        parentBirthConnectionPoint.transform.parent = parentPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        parentBirthConnectionPoint.transform.localPosition = new Vector3(0, 0.5f, myAgeConnectionPointPercent);  // 0.5 top of platform

        var textMeshProItem = parentBirthConnectionPoint.transform.GetComponentsInChildren<TextMeshPro>().First();
        
        if (textMeshProItem.tag.Contains("ChildName"))
            textMeshProItem.text = childPersonNode.person.givenName.Split(' ')[0];
        

        var triggerTeleportToChildScript = parentBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
        triggerTeleportToChildScript.teleportTargetChild = childPlatformTransform;
        triggerTeleportToChildScript.teleportOffset = new Vector3(0, 2.5f, 0);
        triggerTeleportToChildScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
        triggerTeleportToChildScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;

        childBirthConnectionPoint = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);
        renderer = childBirthConnectionPoint.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_BaseColor", clearWhite);
        childBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        childBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
        childBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, childAgeConnectionPointPercent);

        // Mom and Dad Return transportes
        if (this.personGender == PersonGenderType.Male)
        {
            returnToFatherBirthConnectionPoint =
                Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToFatherRenderer = returnToFatherBirthConnectionPoint.GetComponentInChildren<Renderer>(); 
            returnToFatherRenderer.material.SetColor("_BaseColor", blue);
            var triggerTeleportToFatherScript = returnToFatherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToFatherScript.teleportTargetChild = parentPlatformTransform;
            triggerTeleportToFatherScript.teleportOffset = new Vector3(-3f, 2.5f, myAgeConnectionPointPercent * this.lifeSpan * 5);
            triggerTeleportToFatherScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
            triggerTeleportToFatherScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;

            //returnToFatherBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
            returnToFatherBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
            returnToFatherBirthConnectionPoint.transform.localPosition = new Vector3(-3f, 0, 0);
        }
        else if (this.personGender == PersonGenderType.Female)
        {
            returnToMotherBirthConnectionPoint =
                Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToMotherRenderer = returnToMotherBirthConnectionPoint.GetComponentInChildren<Renderer>();
            returnToMotherRenderer.material.SetColor("_BaseColor", pink);
            var triggerTeleportToMotherScript = returnToMotherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToMotherScript.teleportTargetChild = parentPlatformTransform;
            triggerTeleportToMotherScript.teleportOffset = new Vector3(3f, 2.5f, myAgeConnectionPointPercent * this.lifeSpan * 5);
            triggerTeleportToMotherScript.hallOfHistoryGameObject = hallOfHistoryGameObject;
            triggerTeleportToMotherScript.hallOfFamilyPhotosGameObject = hallOfFamilyPhotosGameObject;

            //returnToMotherBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
            returnToMotherBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
            returnToMotherBirthConnectionPoint.transform.localPosition = new Vector3(3f, 0, 0);
        }

        GameObject edge = Instantiate(this.birthConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Birth {birthDate} {childPersonNode.name}";
        edge.GetComponent<Edge>().CreateEdge(parentBirthConnectionPoint, childBirthConnectionPoint);

        edge.transform.GetChild(ScaleThisChildIndex).GetComponent<Renderer>().material.SetColor("_BaseColor",
            childRelationshipColors[(int)childRelationshipType]);

        edge.transform.parent = transform;
    }

    public void AddMarriageEdge(PersonNode spousePersonNode,
        float myAgeConnectionPointPercent = 0f,
        float spouseAgeConnectionPointPercent = 0f, int marriageEventDate = 0, int marriageLength = 0)
    {
        var myPositionThisPlatformTransform = gameObject.transform;
        var myRidgidbodyComponent = myPositionThisPlatformTransform.GetComponent<Rigidbody>();
        var spousePositionThisPlatformTransform = spousePersonNode.transform;
        var spouseRidgidbodyComponent = spousePositionThisPlatformTransform.GetComponent<Rigidbody>();
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);   // - 0.5f
            sj.connectedAnchor = new Vector3(0, 0.5f, spouseAgeConnectionPointPercent);  // - 0.5f
            sj.enableCollision = true;
            sj.connectedBody = spouseRidgidbodyComponent;
            sj.spring = 5f; // 50.0f;
            sj.minDistance = 20.0f;
            sj.maxDistance = 80.0f;
        }
        parentBirthConnectionPoint = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //leftConnection.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.yellow);

        parentBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        parentBirthConnectionPoint.transform.parent = myPositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        parentBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent);

        childBirthConnectionPoint = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //rightConnection.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.yellow);
        childBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        childBirthConnectionPoint.transform.parent = spousePositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        childBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, spouseAgeConnectionPointPercent);

        GameObject edge = Instantiate(this.marriageConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Marriage {marriageEventDate}, to {spousePersonNode.name}, duration {marriageLength}.";
        edge.GetComponent<Edge>().CreateEdge(parentBirthConnectionPoint, childBirthConnectionPoint);
        edge.GetComponent<Edge>().SetEdgeEventLength(marriageLength * 5, marriageConnectionXScale);
        var material = edge.transform.GetChild(0).GetComponent<Renderer>().material;
        material.SetColor("_BaseColor", new Color(1.0f, 0.92f, 0.01f, 0.6f));
        //material.SetOverrideTag("RenderMode", "Transparent");

        edge.transform.parent = myPositionThisPlatformTransform;
    }

    public void SetDebugAddMotionSetting(bool newDebugAddMotionSetting)
    {
        this.debugAddMotion = newDebugAddMotionSetting;
    }
}