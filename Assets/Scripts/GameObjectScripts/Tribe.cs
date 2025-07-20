using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;
using Assets.Scripts.DataObjects;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;
using Assets.Scripts.ServiceProviders;
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
	//make this a SerializeField
	[SerializeField]
	[Tooltip("Number of generations to load on each side (dynamically expandable)")]
	private int numberOfGenerations = 2;
	private PersonDetailsHandler personDetailsHandlerScript;
	private Transform lastTeleportTransform;
	private Vector3 lastTeleportOffset;
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
	[SerializeField]
	[Tooltip("Enable Hall of History feature on person platforms")]
	private bool EnableHallOfHistory = true;
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
	private Dictionary<int, List<Person>> listOfPersonsPerGeneration = new Dictionary<int, List<Person>>();
	const int PlatformChildIndex = 0;

	private ThirdPersonController thirdPersonController;
	private bool controllerSubscribed = false;

	// A note about PlayerInput:
	// Unity'y PlayerInput is persnickety about shared input devices and I can not use PlayerIntput here because it will cause problems for the ThirdPersonController.
	// For example, the PlaerInput on the ThirdPersonController is configured to use the Gamepad input device as well as the UI/Keyboard device.
	// Adding a playerInput to the Tribe GameObject that connects via the UI/Keyboard device will cause the Keyboard input to be ignored
	// by the ThirdPersonController.
	// This is why I am not using PlayerInput for the Tribe GameObject.
	//  I need Gamepad input and Keyboaed to be able to send messages to the ThirdPerson Controller as well as the Trabe Object
	// OnMenu and OnStart are both have amppings for the Gamepad and Keyboard.
	// I will have the ThirdPerson Controller forward the OnMenu and OnStart events to the Tribe GameObject.

	// ** Also note that the ThirdPersonContoller uses the StarterAssetsInputs which are NOT the same as the system wide default

	void Start()
	{
		dataLoadComplete = false;
		updateFramesToWaist = 120;
		tribeType = CrossSceneInformation.myTribeType;
		numberOfGenerations = CrossSceneInformation.numberOfGenerations;
		startingIdForTree = CrossSceneInformation.startingDataBaseId;
		rootsMagicFileName = CrossSceneInformation.rootsMagicDataFileNameWithFullPath;
		digiKamFileName = CrossSceneInformation.digiKamDataFileNameWithFullPath;

		// Initialize the data provider
		_dataProvider = new RootsMagicFamilyHistoryDataProvider();
		var config = new Dictionary<string, string>
		{
			{ PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, rootsMagicFileName }
		};
		_dataProvider.Initialize(config);

		// Find the PersonDetailsHandler component
		personDetailsHandlerScript = FindFirstObjectByType<PersonDetailsHandler>();
		if (personDetailsHandlerScript == null)
		{
			Debug.LogWarning("No PersonDetailsHandler component found in the scene. Some functionality may be limited.");
		}

		if (tribeType == TribeType.MadeUpData || rootsMagicFileName == null)
		{
			var adam = CreatePersonGameObject("Adam", PersonGenderType.Male, 1400, false, 1500, xOffset: 10, generation: 0);

			//SetDemoMotionMode(adam);

			CreatePlayerOnThisPersonObject(adam);

			var eve = CreatePersonGameObject("Eve", PersonGenderType.Female, 1410, false, 1520, xOffset: 30, generation: 0);

			CreateMarriage(eve, adam, 1430);
			dataLoadComplete = true;
      
        } 		
	}

	void EnsureGenerationExists(int generation)
	{
		if (!listOfPersonsPerGeneration.ContainsKey(generation))
		{
			listOfPersonsPerGeneration[generation] = new List<Person>();
		}
	}

	void PositionTimeBarrier()
	{
		var timeBarrierObject = GameObject.FindGameObjectsWithTag("TimeBarrier")[0];
		if (timeBarrierYear == 0)
			timeBarrierYear = DateTime.Now.Year;
		// Position at the end of the timeBarrierYear (add 5 to place it at the end of the year instead of beginning)
		// Subtract 0.1 to prevent walking to the very end from triggering the next year
		timeBarrierObject.transform.position = new Vector3(0f, 0f, (timeBarrierYear + 1) * 5 - 0.1f);
		timeBarrierObject.transform.localScale = new Vector3((maximumNumberOfPeopleInAGeneration * personSpacing * 10f), 0.1f, (maximumNumberOfPeopleInAGeneration * personSpacing * 10f));
	}

	private IEnumerator GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(int personId, int depth, float xOffSet, float xRange, bool pleaseSkipStartingPerson = false)
	{
		var personList = _dataProvider.GetPerson(personId, generation: depth, xOffSet + xRange / 2, spouseNumber: 0);
		if (personList.Count > 0)
		{
			var personWeAreAdding = personList[0];
			//only add the person if we are not skipping the starting person and the person is not the starting person
			if (!(pleaseSkipStartingPerson && personWeAreAdding.dataBaseOwnerId == startingIdForTree))
			{
				if (!PersonExistsInGeneration(personWeAreAdding.dataBaseOwnerId, depth))
				{
					EnsureGenerationExists(depth);
					listOfPersonsPerGeneration[depth].Add(personWeAreAdding);
				}
			}

			var listOfFamilyIds = AddParentsAndFixUpDates(personWeAreAdding);
			if (depth > 0)
			{
				var parentCount = listOfFamilyIds.Count;
				var parentIndex = 0;
				foreach (var familyId in listOfFamilyIds)
				{
					var newRange = xRange / parentCount;
					var newOffset = xOffSet + parentIndex * newRange;

					StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(familyId, depth - 1, newOffset, newRange, pleaseSkipStartingPerson));
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
			if (!PersonExistsInGeneration(personWeAreAdding.dataBaseOwnerId, numberOfGenerations - depth - centerByThisOffset))
			{
				EnsureGenerationExists(numberOfGenerations - depth - centerByThisOffset);
				listOfPersonsPerGeneration[numberOfGenerations - depth - centerByThisOffset].Add(personWeAreAdding);
			}

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

	private bool PersonExistsInGeneration(int personId, int depth)
	{
		if (!listOfPersonsPerGeneration.ContainsKey(depth))
			return false;
		return listOfPersonsPerGeneration[depth].Any(p => p.dataBaseOwnerId == personId);
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
				if (!PersonExistsInGeneration(spousePersonWeAreAdding.dataBaseOwnerId, depth))
				{
					EnsureGenerationExists(depth);
					listOfPersonsPerGeneration[depth].Add(spousePersonWeAreAdding);
				}
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
		foreach (var depth in listOfPersonsPerGeneration.Keys)
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
		foreach (var depth in listOfPersonsPerGeneration.Keys)
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
		foreach (var depth in listOfPersonsPerGeneration.Keys)
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
		foreach (var depth in listOfPersonsPerGeneration.Keys.Where(k => k >= 0).OrderBy(k => k))
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
				listOfPersonsPerGeneration.ContainsKey(depth) ? 
				listOfPersonsPerGeneration[depth].FirstOrDefault(x => x.dataBaseOwnerId == ownerId)?.personNodeGameObject : null;
	Person getPersonForDataBaseOwnerId(int ownerId, int depth) =>
				listOfPersonsPerGeneration.ContainsKey(depth) ? 
				listOfPersonsPerGeneration[depth].FirstOrDefault(x => x.dataBaseOwnerId == ownerId) : null;

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
		//personObjectScript.SetThumbnailForPerson(rootsMagicFileName, digiKamFileName);
		if (EnableHallOfHistory)
		{
			personObjectScript.SetHallOfHistoryGameObject(hallOfHistoryGameObject);
		}
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

		playerGameObject.transform.SetParent(personGameObject.transform, false);
		
		playerGameObject.transform.position = new Vector3(0f, 1f, 0f);

		playerGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

		GameObject[] targets = GameObject.FindGameObjectsWithTag("CinemachineTarget");
		GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(playerGameObject.transform));

		CreatePlayerFollowCameraObject(target);

		var teleporter = playerGameObject.GetComponent<ThirdPersonTeleporter>();
		teleporter.TeleportTo(personGameObject.transform, new Vector3(0,0.5f,0), ticksToHoldHere: 25);

		var personObjectScript = personGameObject.GetComponent<PersonNode>();

		if (EnableHallOfHistory)
		{
			StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(personObjectScript));
		}
		// Enable Hall of Family Photos
		StartCoroutine(hallOfFamilyPhotosGameObject.GetComponent<HallOfFamilyPhotos>().SetFocusPersonNode(personObjectScript));
		
		return playerGameObject;
	}

	private void CreatePlayerFollowCameraObject(GameObject target)
	{
		var playerFollowCameraGameObject = Instantiate(playerFollowCameraPrefab);

		var vCam = playerFollowCameraGameObject.GetComponent<CinemachineCamera>();
		vCam.Follow = target.transform;
		vCam.LookAt = target.transform;
		vCam.Target.TrackingTarget = target.transform;
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

	void CreateMarriage(GameObject wifePerson, GameObject husbandPerson, int marriageEventDate, bool divorcedFlag = false, int divorcedEventDate = 0)
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
		var husbandMarriageConnectionPointPercent = husbandAge != 0f ? husbandAgeAtMarriage / husbandAge : 0.5f;

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
			if (motherAgeAtChildBirth == 0) {
				Debug.LogWarning($"OwnerId={motherPersonNode.dataBaseOwnerID} Name={motherPersonNode.name} Mother age at child birth is zero, setting to 13");
				motherAgeAtChildBirth = 13;
			}
			// protect against motherAge being zero
			if (motherAge == 0)
				motherAge = motherAgeAtChildBirth;
			motherPersonNode.AddBirthEdge(childPersonNode, motherAgeAtChildBirth / motherAge, motherChildRelationshipType, childPersonNode.birthDate);
		}

		if (fatherPerson != null && !fatherPerson.Equals(null))
		{
			var fatherPersonNode = fatherPerson.GetComponent<PersonNode>();
			var fatherAge = fatherPersonNode.lifeSpan;
			var fatherAgeAtChildBirth = (float)(childPersonNode.birthDate - fatherPersonNode.birthDate);
			if (fatherAgeAtChildBirth == 0) {
				Debug.LogWarning($"OwnerId={fatherPersonNode.dataBaseOwnerID} Name={fatherPersonNode.name} Father age at child birth is zero, setting to 13");
				fatherAgeAtChildBirth = 13;
			}
			// protect against fatherAge being zero
			if (fatherAge == 0)
				fatherAge = fatherAgeAtChildBirth;
			fatherPersonNode.AddBirthEdge(childPersonNode, fatherAgeAtChildBirth / fatherAge, fatherChildRelationshipType, childPersonNode.birthDate);
		}
	}

	void Update()
	{
		if (!controllerSubscribed)
		{
			thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
			if (thirdPersonController != null)
			{
				SubscribeToControllerEvents();
			}
		}

		if (dataLoadComplete)
			return;

		if (updateFramesToWaist-- > 0)
			return;

		if (tribeType == TribeType.Ancestry)
		{
			// Clear any existing data
			listOfPersonsPerGeneration.Clear();
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
			// Clear any existing data
			listOfPersonsPerGeneration.Clear();
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
			var generationsOnEachSide = numberOfGenerations;
			// Centered puts a generation on each side minimum
			numberOfGenerations = generationsOnEachSide + generationsOnEachSide + 1;
			// Clear any existing data
			listOfPersonsPerGeneration.Clear();
			StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(startingIdForTree, generationsOnEachSide, xOffSet: 0.0f, xRange: 1.0f, centerByThisOffset: 1));
			// With the centered tribe, we want to skip the starting person on the ancestry side
			StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(startingIdForTree, generationsOnEachSide, xOffSet: 0.0f, xRange: 1.0f, pleaseSkipStartingPerson:true));

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

	private void SubscribeToControllerEvents()
	{
		if (thirdPersonController != null && !controllerSubscribed)
		{
			thirdPersonController.onMenuPressed.AddListener(OnMenu);
			thirdPersonController.onStartPressed.AddListener(OnStart);
			controllerSubscribed = true;
		}
	}

	void OnDestroy()
	{
		if (thirdPersonController != null)
		{
			thirdPersonController.onMenuPressed.RemoveListener(OnMenu);
			thirdPersonController.onStartPressed.RemoveListener(OnStart);
		}
	}

	private void OnMenu()
	{
		SceneManager.LoadScene("aaStart RootsMagicNamePicker");
	}

	private void OnStart()
	{
		if (personDetailsHandlerScript != null)
			personDetailsHandlerScript.OnStartInputAction();
	}

	private void OnMenuCanceled()
	{
		Debug.Log("OnMenu canceled");
	}

	private void OnStartCanceled()
	{
		Debug.Log("OnStart canceled");
	}

	public void LoadNextLevelOfDescendancyForPerson(int personId, int currentGeneration, PersonGenderType personGender)
	{
		bool thisIsAHusbandQuery = personGender == PersonGenderType.Male;
		// Get the person's marriages
		var marriages = _dataProvider.GetMarriages(personId, useHusbandQuery: thisIsAHusbandQuery);
		
		// Get the person who initiated this expansion for date fixing
		var originatingPerson = getPersonForDataBaseOwnerId(personId, currentGeneration);
		
		foreach (var marriage in marriages)
		{
			// Get correct spouse ID based on the person's gender
			var spouseId = thisIsAHusbandQuery ? marriage.wifeId : marriage.husbandId;
			var spouseAlreadyExists = PersonExistsInGeneration(spouseId, currentGeneration);
			Person spousePerson = null;
			
			if (!spouseAlreadyExists)
			{
				// Calculate appropriate xOffset for spouse placement
				float spouseXOffset = CalculateNextAvailableXOffset(currentGeneration);
				
				var spouseList = _dataProvider.GetPerson(spouseId, generation: currentGeneration, spouseXOffset, spouseNumber: 0);
				if (spouseList.Count > 0)
				{
					spousePerson = spouseList[0];
					EnsureGenerationExists(currentGeneration);
					listOfPersonsPerGeneration[currentGeneration].Add(spousePerson);
					spousePerson.personNodeGameObject = CreatePersonGameObject(spousePerson, globalSpringType);
					
					// Refresh positioning for the current generation after adding spouse
					RefreshGenerationPositioning(currentGeneration);
				}
			}
			else
			{
				spousePerson = getPersonForDataBaseOwnerId(spouseId, currentGeneration);
			}
			
			// Apply the same date fixing logic as in initial setup
			if (originatingPerson != null && spousePerson != null)
			{
				originatingPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, spousePerson);
				spousePerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, originatingPerson);
				
				// Create the marriage connection (visual connection between spouses)
				int marriageYearToUse = originatingPerson.FixUpAndReturnMarriageDate(marriage.marriageYear);
				bool divorcedOrAnnuledFlag = marriage.divorcedYear != 0 || marriage.annulledYear != 0;
				int divorcedOrAnnuledDate = marriage.divorcedYear != 0 ? marriage.divorcedYear : marriage.annulledYear;
				
				CreateMarriage(
					thisIsAHusbandQuery ? spousePerson.personNodeGameObject : originatingPerson.personNodeGameObject, // wife
					thisIsAHusbandQuery ? originatingPerson.personNodeGameObject : spousePerson.personNodeGameObject, // husband
					marriageYearToUse,
					divorcedOrAnnuledFlag,
					divorcedOrAnnuledDate);
			}

			// Get children for this marriage
			var children = _dataProvider.GetChildren(marriage.familyId);
			var nextGeneration = currentGeneration + 1;
			var childCount = children.Count;
			var childIndex = 0;

			// Calculate starting xOffset for new children - place them to the right of existing people
			float startingXOffset = CalculateNextAvailableXOffset(nextGeneration);

			foreach (var child in children)
			{
				if (!PersonExistsInGeneration(child.childId, nextGeneration))
				{
					// Calculate xOffset for this child to ensure proper spacing
					float childXOffset = startingXOffset + (childIndex * 0.1f); // 0.1f spacing between siblings
					
					var childList = _dataProvider.GetPerson(child.childId, generation: nextGeneration, childXOffset, spouseNumber: 0);
					if (childList.Count > 0)
					{
						var childPerson = childList[0];
						EnsureGenerationExists(nextGeneration);
						listOfPersonsPerGeneration[nextGeneration].Add(childPerson);
						childPerson.personNodeGameObject = CreatePersonGameObject(childPerson, globalSpringType);

						// Create parent-child connections
						var parentPerson = listOfPersonsPerGeneration[currentGeneration].FirstOrDefault(p => p.dataBaseOwnerId == personId);
						var parentSpouse = listOfPersonsPerGeneration[currentGeneration].FirstOrDefault(p => p.dataBaseOwnerId == spouseId);
						
						if (parentPerson != null && childPerson.personNodeGameObject != null)
						{
							AssignParents(
								childPerson.personNodeGameObject,
								parentSpouse?.personNodeGameObject,
								parentPerson.personNodeGameObject,
								child.relationToMother,
								child.relationToFather
							);
						}
					}
				}
				childIndex++;
			}
			
			// After adding new people for this marriage, refresh positioning for this generation
			if (children.Count > 0)
			{
				RefreshGenerationPositioning(nextGeneration);
			}
		}
	}

	/// <summary>
	/// Calculate the next available xOffset for a generation, placing new people to the right
	/// </summary>
	private float CalculateNextAvailableXOffset(int generation)
	{
		if (!listOfPersonsPerGeneration.ContainsKey(generation) || listOfPersonsPerGeneration[generation].Count == 0)
		{
			return 0.0f; // Start at 0 if generation is empty
		}

		// Find the maximum xOffset currently in this generation
		float maxXOffset = listOfPersonsPerGeneration[generation].Max(p => p.xOffset);
		
		// Add some spacing to place new people to the right
		return maxXOffset + 0.2f; // 0.2f gap before starting new children
	}

	/// <summary>
	/// Refresh positioning for all people in a generation after dynamic loading
	/// </summary>
	private void RefreshGenerationPositioning(int generation)
	{
		if (!listOfPersonsPerGeneration.ContainsKey(generation))
			return;

		var numberOfPersonsInThisGeneration = listOfPersonsPerGeneration[generation].Count;
		var indexIntoPersonsInThisGeneration = 0;

		// Re-sort and re-index all people in this generation
		foreach (var personToAdd in listOfPersonsPerGeneration[generation].OrderBy(x => x.xOffset))
		{
			personToAdd.numberOfPersonsInThisGeneration = numberOfPersonsInThisGeneration;
			personToAdd.indexIntoPersonsInThisGeneration = indexIntoPersonsInThisGeneration;

			// Recalculate world position using the same logic as initial setup
			if (personToAdd.personNodeGameObject != null)
			{
				var x = indexIntoPersonsInThisGeneration * personSpacing - (numberOfPersonsInThisGeneration * personSpacing) / 2 + personSpacing / 2;
				var currentPos = personToAdd.personNodeGameObject.transform.position;
				personToAdd.personNodeGameObject.transform.position = new Vector3(x, currentPos.y, currentPos.z);
			}

			indexIntoPersonsInThisGeneration++;
		}
	}

	/// <summary>
	/// Dynamically load parents for the specified person and create parent-child connections
	/// </summary>
	public void LoadNextLevelOfAncestryForPerson(int personId, int currentGeneration)
	{
		// Get the parents of the specified person
		var parentsList = _dataProvider.GetParents(personId);
		
		if (parentsList.Count == 0)
			return; // No parents to load
		
		var parentGeneration = currentGeneration - 1;
		
		// Ensure the parent generation list exists
		EnsureGenerationExists(parentGeneration);
		
		// Get the child person for creating connections
		var childPerson = getPersonForDataBaseOwnerId(personId, currentGeneration);
		
		foreach (var parentage in parentsList)
		{
			Person motherPerson = null;
			Person fatherPerson = null;
			
			// Add mother if she doesn't exist
			if (parentage.motherId != 0 && !PersonExistsInGeneration(parentage.motherId, parentGeneration))
			{
				float motherXOffset = CalculateNextAvailableXOffset(parentGeneration);
				var motherList = _dataProvider.GetPerson(parentage.motherId, generation: parentGeneration, motherXOffset, spouseNumber: 0);
				if (motherList.Count > 0)
				{
					motherPerson = motherList[0];
					EnsureGenerationExists(parentGeneration);
					listOfPersonsPerGeneration[parentGeneration].Add(motherPerson);
					motherPerson.personNodeGameObject = CreatePersonGameObject(motherPerson, globalSpringType);
				}
			}
			else if (parentage.motherId != 0)
			{
				motherPerson = getPersonForDataBaseOwnerId(parentage.motherId, parentGeneration);
			}
			
			// Add father if he doesn't exist
			if (parentage.fatherId != 0 && !PersonExistsInGeneration(parentage.fatherId, parentGeneration))
			{
				float fatherXOffset = CalculateNextAvailableXOffset(parentGeneration);
				var fatherList = _dataProvider.GetPerson(parentage.fatherId, generation: parentGeneration, fatherXOffset, spouseNumber: 0);
				if (fatherList.Count > 0)
				{
					fatherPerson = fatherList[0];
					EnsureGenerationExists(parentGeneration);
					listOfPersonsPerGeneration[parentGeneration].Add(fatherPerson);
					fatherPerson.personNodeGameObject = CreatePersonGameObject(fatherPerson, globalSpringType);
				}
			}
			else if (parentage.fatherId != 0)
			{
				fatherPerson = getPersonForDataBaseOwnerId(parentage.fatherId, parentGeneration);
			}
			
			// Create parent-child connections if we have the child and at least one parent
			if (childPerson?.personNodeGameObject != null)
			{
				AssignParents(
					childPerson.personNodeGameObject,
					motherPerson?.personNodeGameObject,
					fatherPerson?.personNodeGameObject,
					parentage.relationToMother,
					parentage.relationToFather
				);
			}
		}
		
		// Refresh positioning for the parent generation after adding new people
		if (parentsList.Count > 0)
		{
			RefreshGenerationPositioning(parentGeneration);
		}
	}
}