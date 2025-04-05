using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;
using Assets.Scripts.DataObjects;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine.SceneManagement;

public class Tribe : MonoBehaviour
{
	private TribeType tribeType;
	private String rootsMagicFileName;
	private String digiKamFileName;
	private bool dataLoadComplete = false;
	private int updateFramesToWaist = 120;
	private int startingIdForTree;
	private int numberOfGenerations = 5;
	[SerializeField]
	[Tooltip("Time Barrier year, 0 for current year")]
	private int timeBarrierYear = 0;
	public GameObject personPrefab;
	[SerializeField]
	[Tooltip("PlayerJasper Prefab goes here")]
	public GameObject playerControllerPrefab;
	public GameObject playerFollowCameraPrefab;
	public GameObject birthConnectionPrefab;
	public GameObject marriageConnectionPrefab;
	public GameObject hallOfHistoryGameObject;
	public GameObject hallOfFamilyPhotosGameObject;
	public float marriageEdgepfXScale = 0.4f;
	public GameObject bubblepf;
	public GameObject parentPlatformBirthBubble;
	public GameObject childPlatformReturnToParent;
	public int numberOfPeopleInTribe = 1000;
	public GlobalSpringType globalSpringType = GlobalSpringType.Normal;
	public int generationGap;
	public int spouseGap;
	public int personSpacing = 20;

	private int maximumNumberOfPeopleInAGeneration = 0;
	private IFamilyHistoryDataProvider _dataProvider;
	private List<Person>[] listOfPersonsPerGeneration = new List<Person>[25];
	private int personOfInterestDepth = 0;
	private int personOfInterestIndexInList = 0;

	const int PlatformChildIndex = 0;
	private StarterAssets.ThirdPersonController thirdPersonContollerScript;
	private StarterAssetsInputs _input;

	void Start()
	{
		dataLoadComplete = false;
		updateFramesToWaist = 120;
		tribeType = Assets.Scripts.CrossSceneInformation.myTribeType;
		numberOfGenerations = Assets.Scripts.CrossSceneInformation.numberOfGenerations;
		startingIdForTree = Assets.Scripts.CrossSceneInformation.startingDataBaseId;
		rootsMagicFileName = Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;
		digiKamFileName = Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath;

		// Initialize the data provider
		_dataProvider = new RootsMagicFamilyHistoryDataProvider();
		var config = new Dictionary<string, string>
		{
			{ "RootsMagicDatabasePath", rootsMagicFileName }
		};
		_dataProvider.Initialize(config);

		if (tribeType == TribeType.MadeUpData || rootsMagicFileName == null)
		{
			var adam = CreatePersonGameObject("Adam", PersonGenderType.Male, 1400, false,1500, xOffset:10, generation: 0);

			//SetDemoMotionMode(adam);

			CreatePlayerOnThisPersonObject(adam);

			var eve = CreatePersonGameObject("Eve", PersonGenderType.Female, 1410, false, 1520, xOffset: 30, generation: 0);

            CreateMarriage(eve, adam, 1430);
			dataLoadComplete = true;
      
        } 		
	}

	void NewUpEnoughListOfPersonsPerGeneration(int numberOfGenerations)
    {
		for(var depth = 0; depth <= numberOfGenerations; depth++)
        {
			listOfPersonsPerGeneration[depth] = new List<Person>();
        }
    }

	void PositionTimeBarrier()
    {
		var timeBarrierObject = GameObject.FindGameObjectsWithTag("TimeBarrier")[0];
		if (timeBarrierYear == 0)
			timeBarrierYear = DateTime.Now.Year;
		timeBarrierObject.transform.position = new Vector3(0f, 0f, timeBarrierYear * 5 + 0.5f);
		timeBarrierObject.transform.localScale = new Vector3((maximumNumberOfPeopleInAGeneration * personSpacing * 10f), 0.1f, (maximumNumberOfPeopleInAGeneration * personSpacing * 10f));
	}

	private IEnumerator GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(int personId, int depth, float xOffSet, float xRange)
	{
		var personList = _dataProvider.GetPerson(personId, generation: depth, xOffSet + xRange / 2, spouseNumber: 0);
		if (personList.Count > 0)
		{
			var personWeAreAdding = personList[0];
			listOfPersonsPerGeneration[depth].Add(personWeAreAdding);

			var listOfFamilyIds = AddParentsAndFixUpDates(personWeAreAdding);
			if (depth > 0)
			{
				var parentCount = listOfFamilyIds.Count;
				var parentIndex = 0;
				foreach (var familyId in listOfFamilyIds)
				{
					var newRange = xRange / parentCount;
					var newOffset = xOffSet + parentIndex * newRange;

					StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(familyId, depth - 1, newOffset, newRange));
					parentIndex++;
				}
			}
		}
		yield return null;
	}

	private IEnumerator GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(int personId, int depth, float xOffSet, float xRange, int centerByThisOffset = 0)
	{
		var personList = _dataProvider.GetPerson(personId, generation: numberOfGenerations - depth - centerByThisOffset, xOffSet + xRange / 2, spouseNumber: 0);
		if (personList.Count > 0)
		{
			var personWeAreAdding = personList[0];
			listOfPersonsPerGeneration[numberOfGenerations - depth - centerByThisOffset].Add(personWeAreAdding);

			var listOfFamilyIds = AddSpousesAndFixUpDates(personWeAreAdding, numberOfGenerations - depth - centerByThisOffset, xOffSet, xRange);
			if (depth > 0)
			{
				foreach (var familyId in listOfFamilyIds)
				{
					var children = _dataProvider.GetChildren(familyId);
					var childCount = children.Count;
					var childIndex = 0;

					foreach (var child in children)
					{
						var newRange = xRange / childCount;
						var newOffset = xOffSet + childIndex * newRange;
						StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(child.childId, depth - 1, newOffset, newRange, centerByThisOffset));
						childIndex++;
					}
				}
			}
		}
		yield return null;
	}

	List<int> AddParentsAndFixUpDates(Person forThisPerson)
	{
		var listOfPersonIdsToReturn = new List<int>();
		var parentsList = _dataProvider.GetParents(forThisPerson.dataBaseOwnerId);
		
		foreach (var parentage in parentsList)
		{
			if (parentage.fatherId != 0 && !listOfPersonIdsToReturn.Contains(parentage.fatherId))
				listOfPersonIdsToReturn.Add(parentage.fatherId);
			if (parentage.motherId != 0 && !listOfPersonIdsToReturn.Contains(parentage.motherId))
				listOfPersonIdsToReturn.Add(parentage.motherId);
		}
		return listOfPersonIdsToReturn;
	}

	List<int> AddSpousesAndFixUpDates(Person forThisPerson, int depth, float xOffset, float xRange)
	{
		var listOfFamilyIdsToReturn = new List<int>();
		bool thisIsAHusbandQuery = (forThisPerson.gender == PersonGenderType.Male);
		
		var marriages = _dataProvider.GetMarriages(forThisPerson.dataBaseOwnerId, useHusbandQuery: thisIsAHusbandQuery);
		int spouseNumber = 1;

		foreach (var marriage in marriages)
		{
			var spouseIdWeAreAdding = thisIsAHusbandQuery ? marriage.wifeId : marriage.husbandId;

			var spouseXOffset = xOffset + (xRange / (marriages.Count + 2)) * spouseNumber;
			var spouseList = _dataProvider.GetPerson(spouseIdWeAreAdding, generation: depth, spouseXOffset, spouseNumber);
			if (spouseList.Count > 0)
			{
				var spousePersonWeAreAdding = spouseList[0];
				listOfPersonsPerGeneration[depth].Add(spousePersonWeAreAdding);
				
				forThisPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, spousePersonWeAreAdding);
				spousePersonWeAreAdding.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, forThisPerson);
			}
			
			listOfFamilyIdsToReturn.Add(marriage.familyId);
			spouseNumber++;
		}
		return listOfFamilyIdsToReturn;
	}

	void FixUpDatesBasedOffMarriageDates()
    {
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			foreach (var potentialMarriedPerson in listOfPersonsPerGeneration[depth])
			{
				var marriages = _dataProvider.GetMarriages(potentialMarriedPerson.dataBaseOwnerId, 
					useHusbandQuery: potentialMarriedPerson.gender == PersonGenderType.Male);
				
				foreach (var marriage in marriages)
				{
					var spouseId = potentialMarriedPerson.gender == PersonGenderType.Male ? marriage.wifeId : marriage.husbandId;
					var spouse = listOfPersonsPerGeneration[depth].FirstOrDefault(p => p.dataBaseOwnerId == spouseId);
					if (spouse != null)
					{
						potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, spouse);
					}
				}
			}
		}
	}

	void CreatePersonGameObjectForMyTribeOfPeople(int startingID, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
    {
		maximumNumberOfPeopleInAGeneration = 0;
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			var numberOfPersonsInThisGeneration = listOfPersonsPerGeneration[depth].Count;
			if (numberOfPersonsInThisGeneration > maximumNumberOfPeopleInAGeneration)
				maximumNumberOfPeopleInAGeneration = numberOfPersonsInThisGeneration;

			var indexIntoPersonsInThisGeneration = 0;
			foreach (var personToAdd in listOfPersonsPerGeneration[depth].OrderBy(x => x.xOffset))
			{
				personToAdd.numberOfPersonsInThisGeneration = numberOfPersonsInThisGeneration;
				personToAdd.indexIntoPersonsInThisGeneration = indexIntoPersonsInThisGeneration;

				personToAdd.personNodeGameObject = CreatePersonGameObject(personToAdd, globalSpringType);
				if (personToAdd.dataBaseOwnerId == startingID)
					CreatePlayerOnThisPersonObject(personToAdd.personNodeGameObject);
				indexIntoPersonsInThisGeneration++;
			}
		}
	}

	void HookUpTheMarriages()
	{
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			foreach (var potentialHusbandPerson in listOfPersonsPerGeneration[depth])
			{
				if (potentialHusbandPerson.gender == PersonGenderType.Male)
				{
					var marriages = _dataProvider.GetMarriages(potentialHusbandPerson.dataBaseOwnerId);
					foreach (var marriage in marriages)
					{
						int marriageYearToUse = potentialHusbandPerson.FixUpAndReturnMarriageDate(marriage.marriageYear);
						bool divorcedOrAnnuledFlag = marriage.divorcedYear != 0 || marriage.annulledYear != 0;
						int divorcedOrAnnuledDate = marriage.divorcedYear != 0 ? marriage.divorcedYear : marriage.annulledYear;

						CreateMarriage(
							getGameObjectForDataBaseOwnerId(marriage.wifeId, depth),
							getGameObjectForDataBaseOwnerId(marriage.husbandId, depth),
							marriageYearToUse,
							divorcedOrAnnuledFlag,
							divorcedOrAnnuledDate);
					}
				}
			}
		}
	}

	void NowAddChildrenAssignments(TribeType tribeType)
	{
		for (var depth = 1; depth <= numberOfGenerations; depth++)
		{
			foreach (var child in listOfPersonsPerGeneration[depth])
			{
				var parents = _dataProvider.GetParents(child.dataBaseOwnerId);
				foreach (var parentage in parents)
				{
					AssignParents(
						getGameObjectForDataBaseOwnerId(child.dataBaseOwnerId, depth),
						getGameObjectForDataBaseOwnerId(parentage.motherId, depth - 1),
						getGameObjectForDataBaseOwnerId(parentage.fatherId, depth - 1),
						parentage.relationToMother,
						parentage.relationToFather);
				}
			}
		}
	}

	GameObject getGameObjectForDataBaseOwnerId(int ownerId, int depth) =>
				listOfPersonsPerGeneration[depth].FirstOrDefault(x => x.dataBaseOwnerId == ownerId)?.personNodeGameObject;
	Person getPersonForDataBaseOwnerId(int ownerId, int depth) =>
				listOfPersonsPerGeneration[depth].FirstOrDefault(x => x.dataBaseOwnerId == ownerId);

	GameObject CreatePersonGameObject(string name, PersonGenderType personGender, int birthEventDate,
		bool isLiving = true, int deathEventDate = 0,
		int generation = 0,
		int numberOfPersonsInThisGeneration = 0,
		int indexIntoPersonsInThisGeneration = 0,
		float xOffset = 0.0f,
		int spouseNumber = 0,
		int originalBirthDate = 0, int originalDeathDate = 0, string dateQualityInformationString = "",
		int databaseOwnerArry = 0, int tribeArrayIndex = 0, GlobalSpringType globalSpringType = GlobalSpringType.Normal,
		Person person = null)
    {
		var currentYear = DateTime.Now.Year;
	
		var age = isLiving ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		// old way    var x = xOffset * (maximumNumberOfPeopleInAGeneration * personSpacing);
		var x = indexIntoPersonsInThisGeneration * personSpacing - (numberOfPersonsInThisGeneration * personSpacing) / 2 + personSpacing / 2;
		var y = generation * generationGap + spouseNumber * spouseGap;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate * 5), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetIndexes(databaseOwnerArry, tribeArrayIndex, person);
		personObjectScript.SetLifeSpan(birthEventDate, age, isLiving);
		personObjectScript.AddDateQualityInformation((birthEventDate, originalBirthDate), (deathEventDate, originalDeathDate), dateQualityInformationString);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(birthConnectionPrefab, marriageConnectionPrefab, bubblepf, parentPlatformBirthBubble, childPlatformReturnToParent, marriageEdgepfXScale);
		personObjectScript.addMyBirthQualityBubble();
		personObjectScript.SetGlobalSpringType(globalSpringType);
		personObjectScript.SetThumbnailForPerson(rootsMagicFileName, digiKamFileName);
		personObjectScript.SetHallOfHistoryGameObject(hallOfHistoryGameObject);
		personObjectScript.SetHallOfFamilyPhotosGameObject(hallOfFamilyPhotosGameObject);

		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;
	}

	void SetDemoMotionMode(GameObject personGameObject)
    {
		personGameObject.GetComponent<PersonNode>().SetDebugAddMotionSetting(true);
	}

	GameObject CreatePlayerOnThisPersonObject(GameObject personGameObject)
    {
		personGameObject.GetComponent<Rigidbody>().isKinematic = true;  // Lets make this one stay put

		GameObject playerGameObject = Instantiate(playerControllerPrefab);

		//playerGameObject.transform.SetParent(personGameObject.transform, false);
		
		//playerGameObject.transform.position = new Vector3(0f, 1f, 0f);

		//playerGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

		GameObject[] targets = GameObject.FindGameObjectsWithTag("CinemachineTarget");
		GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(playerGameObject.transform));
		
		CreatePlayerFollowCameraObject(target);

		// We need a better pattern for this.
		// thirdPersonContollerScript = playerGameObject.GetComponent<StarterAssets.ThirdPersonController>();
		// thirdPersonContollerScript.TeleportTo(personGameObject.transform, new Vector3(0,0.5f,0), ticksToHoldHere: 25);

		var personObjectScript = personGameObject.GetComponent<PersonNode>();

		StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(personObjectScript));
		
		return playerGameObject;
    }

	private void teleportToNextPersonOfInterest()
	{
		personOfInterestIndexInList++;
		for (var depth = personOfInterestDepth; depth <= numberOfGenerations; depth++)
		{
			for (var index = personOfInterestIndexInList; index < listOfPersonsPerGeneration[depth]?.Count; index++)
			{
				if (listOfPersonsPerGeneration[depth][index].dateQualityInformationString != "")
				{
					personOfInterestDepth = depth;
					personOfInterestIndexInList = index;
					// We need a better pattern for this.
					//thirdPersonContollerScript.TeleportTo(listOfPersonsPerGeneration[depth][index].personNodeGameObject.transform, new Vector3(0, 0.5f, 0), ticksToHoldHere: 25);
					var personObjectScript = listOfPersonsPerGeneration[depth][index].personNodeGameObject.GetComponent<PersonNode>();

					StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(personObjectScript));
					return;
				}
			}
			personOfInterestIndexInList = 0;
		}
		personOfInterestDepth = 0;

		if (listOfPersonsPerGeneration[personOfInterestDepth]?.Count > personOfInterestIndexInList)
		{
			// We need a better pattern for this.
			//thirdPersonContollerScript.TeleportTo(listOfPersonsPerGeneration[personOfInterestDepth][personOfInterestIndexInList].personNodeGameObject.transform, new Vector3(0, 0.5f, 0), ticksToHoldHere: 25);
			var personObjectScript = listOfPersonsPerGeneration[personOfInterestDepth][personOfInterestIndexInList].personNodeGameObject.GetComponent<PersonNode>();

			StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(personObjectScript));
		}
	}

	private void CreatePlayerFollowCameraObject(GameObject target)
	{
		var playerFollowCameraGameObject = Instantiate(playerFollowCameraPrefab);

		var vCam = playerFollowCameraGameObject.GetComponent<CinemachineCamera>();
		vCam.Follow = target.transform;
		vCam.LookAt = target.transform;
		vCam.Target.TrackingTarget = target.transform;

/* Third Person Follow Distance Modifier is not needed for Unity 2024.1 MAYBE???
		var vDistanceModifier = playerFollowCameraGameObject.GetComponent<ThirdPersonFollowDistanceModifier>();
		if (vDistanceModifier == null)
			Debug.Log("The Player Follow Camera Prefab needs the Third Person Follow Distance Monifier script added.");
		vDistanceModifier.SetFollow();
*/
	}
		

	GameObject CreatePersonGameObject(Person person, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
	{
		return CreatePersonGameObject($"{person.givenName} {person.surName}", person.gender, person.birthEventDate,
			person.isLiving, person.deathEventDate, person.generation, person.numberOfPersonsInThisGeneration, person.indexIntoPersonsInThisGeneration,
			person.xOffset, person.spouseNumber,
			person.originalBirthEventDateYear, person.originalDeathEventDateYear,
			person.dateQualityInformationString,
			person.dataBaseOwnerId, person.tribeArrayIndex, globalSpringType,
			person);
	}

	void CreateMarriage(GameObject wifePerson, GameObject husbandPerson, int marriageEventDate, bool divorcedFlag= false,int divorcedEventDate=0)
    {
		// We may not have loaded a full set of family information
		// If the husband or wife is not found, skip the marriage
		if (ReferenceEquals(wifePerson, null)
			|| ReferenceEquals(husbandPerson, null))
			return;
		var husbandPersonNode = husbandPerson.GetComponent<PersonNode>();
		var wifePersonNode = wifePerson.GetComponent<PersonNode>();
		var wifeAge = wifePersonNode.lifeSpan;

		// We have some married people with no birthdates
		var wifeAgeAtMarriage = (float)(marriageEventDate - wifePersonNode.birthDate);	
		var husbandAge = husbandPersonNode.lifeSpan;
		// We have some married people with no birthdates
		var husbandAgeAtMarriage = (float)(marriageEventDate - husbandPersonNode.birthDate);
		// TODO does not work for divorcedEventDate = 0
		var marriageLength = divorcedFlag ?
			divorcedEventDate - marriageEventDate : (int)Mathf.Min(wifePersonNode.birthDate + wifeAge, husbandPersonNode.birthDate + husbandAge) - marriageEventDate;
		// Just in case birthdate and ages are zero
		if (marriageLength < 0)
			marriageLength = 1;

		var wifeMarriageConnectionPointPercent = wifeAge != 0f ? wifeAgeAtMarriage / wifeAge : 0.5f;
		var husbandMarriageConnectionPointPercent = husbandAge != 0f ? husbandAgeAtMarriage / husbandAge: 0.5f; 
		
		wifePersonNode.AddMarriageEdge(
			husbandPersonNode, 
			wifeMarriageConnectionPointPercent, 
			husbandMarriageConnectionPointPercent, 
			marriageEventDate,
			marriageLength);
	}

	void AssignParents(GameObject childPerson, GameObject motherPerson, GameObject fatherPerson,
		ChildRelationshipType motherChildRelationshipType = ChildRelationshipType.Biological,
		ChildRelationshipType fatherChildRelationshipType = ChildRelationshipType.Biological)
    {
		var childPersonNode = childPerson.GetComponent<PersonNode>();

		if (motherPerson != null && !motherPerson.Equals(null))
		{
			var motherPersonNode = motherPerson.GetComponent<PersonNode>();
			var motherAge = motherPersonNode.lifeSpan;
			var motherAgeAtChildBirth = (float)(childPersonNode.birthDate - motherPersonNode.birthDate);
			motherPersonNode.AddBirthEdge(childPersonNode, motherAgeAtChildBirth / motherAge, motherChildRelationshipType, childPersonNode.birthDate);
		}

		if (fatherPerson != null && !fatherPerson.Equals(null))
		{
			var fatherPersonNode = fatherPerson.GetComponent<PersonNode>();
			var fatherAge = fatherPersonNode.lifeSpan;
			var fatherAgeAtChildBirth = (float)(childPersonNode.birthDate - fatherPersonNode.birthDate);
			fatherPersonNode.AddBirthEdge(childPersonNode, fatherAgeAtChildBirth / fatherAge, fatherChildRelationshipType, childPersonNode.birthDate);
		}
	}

	void Update() 
	{

		//Detect when the F key is pressed down
		if (Input.GetKeyDown(KeyCode.F))
		{
			Debug.Log("F key was pressed.");
			teleportToNextPersonOfInterest();
		}

		if (dataLoadComplete)
			return;

		if (updateFramesToWaist-- > 0)
			return;

		if (tribeType == TribeType.Ancestry)
		{
			NewUpEnoughListOfPersonsPerGeneration(numberOfGenerations);
			StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f));			
			//Debug.Log("We are done with Ancestry Recurrsion.");
			
			FixUpDatesBasedOffMarriageDates();
			//Debug.Log("We are done with Fix Up Dates Based off marriage.");

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);
			//Debug.Log("We are done with creating game objects.");

			HookUpTheMarriages();
			//Debug.Log("We are done with hooking up marriages.");

			NowAddChildrenAssignments(tribeType);
			//Debug.Log("We are done adding children assignments.");

			PositionTimeBarrier();

			dataLoadComplete = true;
		}
		else if (tribeType == TribeType.Descendancy)
		{
			NewUpEnoughListOfPersonsPerGeneration(numberOfGenerations);
			StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f));
			//Debug.Log("We are done with Descendacy Recurrsion.");

			FixUpDatesBasedOffMarriageDates();
			//Debug.Log("We are done with Fix Up Dates Based off marriage.");

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);
			//Debug.Log("We are done with creating game objects.");

			HookUpTheMarriages();
			//Debug.Log("We are done with hooking up marriages.");

			NowAddChildrenAssignments(tribeType);
			//Debug.Log("We are done adding children assignments.");

			PositionTimeBarrier();

			dataLoadComplete = true;
		}
		else if (tribeType == TribeType.Centered)
        {
			// Lets do a 5/5 split 5 generations of Ancsecters and 5 generations of Descendants
			var generationsOnEachSide = 10;
			numberOfGenerations = generationsOnEachSide + generationsOnEachSide + 1;
			NewUpEnoughListOfPersonsPerGeneration(numberOfGenerations);
			StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(startingIdForTree, generationsOnEachSide, xOffSet: 0.0f, xRange: 1.0f, centerByThisOffset: 1));
			StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(startingIdForTree, generationsOnEachSide, xOffSet: 0.0f, xRange: 1.0f));

			//Debug.Log("We are done with Ancestry Recurrsion.");

			FixUpDatesBasedOffMarriageDates();
			//Debug.Log("We are done with Fix Up Dates Based off marriage.");

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);
			//Debug.Log("We are done with creating game objects.");

			HookUpTheMarriages();
			//Debug.Log("We are done with hooking up marriages.");

			NowAddChildrenAssignments(tribeType);
			//Debug.Log("We are done adding children assignments.");

			PositionTimeBarrier();

			dataLoadComplete = true;
		}
	}
}