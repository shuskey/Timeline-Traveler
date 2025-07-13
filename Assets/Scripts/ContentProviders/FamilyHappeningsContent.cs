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
        
        public FamilyHappeningsContent()
        {
            _dataProvider = new RootsMagicFamilyHistoryDataProvider();
        }
        
        public void Initialize()
        {
            string rootsMagicFileName = Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;
            var config = new Dictionary<string, string>
            {
                { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, rootsMagicFileName }
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
            Debug.Log($"[FamilyHappeningsContent] Starting generation for {focusPerson.givenName} {focusPerson.surName} (ID: {focusPerson.dataBaseOwnerId}) for year {year}");
            
            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("=== FAMILY HAPPENINGS ===");
            sb.AppendLine($"Year: {year}");
            sb.AppendLine($"Focus Person: {focusPerson.givenName} {focusPerson.surName}");
            sb.AppendLine();
            
            // Get close family members
            Debug.Log($"[FamilyHappeningsContent] Getting close family members...");
            var closeFamilyMembers = GetCloseFamilyMembers(focusPerson);
            Debug.Log($"[FamilyHappeningsContent] Found {closeFamilyMembers.Count} close family members");
            
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
        /// Gets all close family members for the focus person
        /// </summary>
        private List<Person> GetCloseFamilyMembers(Person focusPerson)
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
            var birthsThisYear = closeFamilyMembers.Where(p => p.originalBirthEventDateYear == year).ToList();
            
            if (!birthsThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var child in birthsThisYear)
            {
                // Skip if this is the focus person themselves (they weren't born this year in the context of the report)
                if (child.dataBaseOwnerId == focusPerson.dataBaseOwnerId)
                    continue;
                    
                var parents = GetParentsOfPerson(child);
                var relationship = GetRelationshipToFocusPerson(child, focusPerson);
                
                sb.AppendLine($"Mr. and Mrs. {GetParentsNames(parents)} are pleased to announce the birth of their {GetGenderText(child.gender)}, {child.givenName} {child.surName}.");
                sb.AppendLine($"{child.givenName} is the {relationship} of {focusPerson.givenName} {focusPerson.surName}.");
                sb.AppendLine($"Born: {FormatDate(child.originalBirthEventDateMonth, child.originalBirthEventDateDay, child.originalBirthEventDateYear)}");
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
            
            // Remove duplicates
            marriagesThisYear = marriagesThisYear.GroupBy(m => m.familyId).Select(g => g.First()).ToList();
            
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
                    var brideRelationship = GetRelationshipToFocusPerson(bride, focusPerson);
                    var groomRelationship = GetRelationshipToFocusPerson(groom, focusPerson);
                    
                    var brideAge = CalculateAge(bride.originalBirthEventDateYear, marriage.marriageYear);
                    var groomAge = CalculateAge(groom.originalBirthEventDateYear, marriage.marriageYear);
                    
                    sb.AppendLine($"{bride.givenName} {bride.surName}, {brideAge}, {brideRelationship} of {focusPerson.givenName} {focusPerson.surName}, was united in marriage to {groom.givenName} {groom.surName}, {groomAge}, {groomRelationship} of {focusPerson.givenName} {focusPerson.surName}.");
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
            var deathsThisYear = closeFamilyMembers.Where(p => p.originalDeathEventDateYear == year && !p.isLiving).ToList();
            
            if (!deathsThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var deceased in deathsThisYear)
            {
                var relationship = GetRelationshipToFocusPerson(deceased, focusPerson);
                var age = CalculateAge(deceased.originalBirthEventDateYear, deceased.originalDeathEventDateYear);
                
                sb.AppendLine($"{deceased.givenName} {deceased.surName}, {age}, {relationship} of {focusPerson.givenName} {focusPerson.surName}, passed away on {FormatDate(deceased.originalDeathEventDateMonth, deceased.originalDeathEventDateDay, deceased.originalDeathEventDateYear)}.");
                sb.AppendLine($"Born: {FormatDate(deceased.originalBirthEventDateMonth, deceased.originalBirthEventDateDay, deceased.originalBirthEventDateYear)}");
                
                // Get tight family of the deceased
                var tightFamily = GetTightFamilyMembers(deceased);
                
                // Those who preceded them in death
                var preceded = tightFamily.Where(p => !p.isLiving && p.originalDeathEventDateYear < deceased.originalDeathEventDateYear && p.dataBaseOwnerId != deceased.dataBaseOwnerId).ToList();
                if (preceded.Any())
                {
                    sb.AppendLine($"Preceded in death by: {string.Join(", ", preceded.Select(p => $"{p.givenName} {p.surName} ({GetRelationshipToDeceased(p, deceased)}, {p.originalDeathEventDateYear})"))}");
                }
                
                // Those who survived them
                var survived = tightFamily.Where(p => p.isLiving && p.dataBaseOwnerId != deceased.dataBaseOwnerId).ToList();
                if (survived.Any())
                {
                    sb.AppendLine($"Survived by: {string.Join(", ", survived.Select(p => $"{p.givenName} {p.surName} ({GetRelationshipToDeceased(p, deceased)})"))}");
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
        private string GetRelationshipToFocusPerson(Person person, Person focusPerson)
        {
            if (person.dataBaseOwnerId == focusPerson.dataBaseOwnerId)
                return "self";
            
            // This is a simplified relationship calculator
            // In a real implementation, you'd want a more sophisticated relationship calculator
            // For now, we'll return basic relationships
            
            // Check if person is a descendant
            if (IsDescendant(person, focusPerson))
                return GetDescendantRelationship(person, focusPerson);
            
            // Check if person is an ancestor
            if (IsAncestor(person, focusPerson))
                return GetAncestorRelationship(person, focusPerson);
            
            // Check if person is a sibling
            if (IsSibling(person, focusPerson))
                return GetSiblingRelationship(person, focusPerson);
            
            // Check if person is a spouse
            if (IsSpouse(person, focusPerson))
                return GetSpouseRelationship(person, focusPerson);
            
            // Default to a generic relationship
            return "relative";
        }

        private string GetRelationshipToDeceased(Person person, Person deceased)
        {
            // Similar to GetRelationshipToFocusPerson but for deceased person
            if (person.dataBaseOwnerId == deceased.dataBaseOwnerId)
                return "self";
            
            if (IsDescendant(person, deceased))
                return GetDescendantRelationship(person, deceased);
            
            if (IsAncestor(person, deceased))
                return GetAncestorRelationship(person, deceased);
            
            if (IsSibling(person, deceased))
                return GetSiblingRelationship(person, deceased);
            
            if (IsSpouse(person, deceased))
                return GetSpouseRelationship(person, deceased);
            
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

        // Utility methods
        private string GetParentsNames(List<Person> parents)
        {
            if (parents.Count == 0) return "Unknown";
            if (parents.Count == 1) return $"{parents[0].givenName} {parents[0].surName}";
            
            var father = parents.FirstOrDefault(p => p.gender == PersonGenderType.Male);
            var mother = parents.FirstOrDefault(p => p.gender == PersonGenderType.Female);
            
            if (father != null && mother != null)
                return $"{father.givenName} and {mother.givenName} {father.surName}";
            
            return string.Join(" and ", parents.Select(p => $"{p.givenName} {p.surName}"));
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
    }
}
