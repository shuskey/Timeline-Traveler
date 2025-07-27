using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using Assets.Scripts.ServiceProviders;

namespace Assets.Scripts.ContentProviders
{
    public class FamilyHappeningsContent
    {
        private FamilyDAG _familyDAG;
        private bool showDataBaseOwnerId = false; // Flag to show database ID after names
        
        public FamilyHappeningsContent()
        {
        }

        /// <summary>
        /// Set the Family DAG for efficient relationship queries
        /// </summary>
        public void SetFamilyDAG(FamilyDAG familyDAG)
        {
            _familyDAG = familyDAG;
        }
        
        public void Initialize()
        {
            // No longer needed - DAG is set via SetFamilyDAG
        }

        /// <summary>
        /// Generates Family Happenings content for a focus person and year
        /// </summary>
        /// <param name="focusPerson">The person to center the report around</param>
        /// <param name="year">The year to generate happenings for</param>
        /// <returns>Formatted Family Happenings report</returns>
        public string GetFamilyHappeningsContent(Person focusPerson, int year)
        {
            if (_familyDAG == null)
            {
                return "Error: Family DAG not initialized. Please set the DAG before generating content.";
            }

            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("=== FAMILY HAPPENINGS ===");
            var focusPersonAge = CalculateAge(focusPerson.originalBirthEventDateYear, year);
            sb.AppendLine($"For: {FormatPersonName(focusPerson)} (age {focusPersonAge.Replace(" years old", "").Replace("infant", "0")})");
            sb.AppendLine();
            
            // Get close family members
            var closeFamilyMembers = GetCloseFamilyMembers(focusPerson);
            
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
        /// Gets all close family members for the focus person according to FamilyDefinitions.md
        /// </summary>
        private List<Person> GetCloseFamilyMembers(Person focusPerson)
        {
            var closeFamilyMembers = new HashSet<Person>();
            

            
            // Add the focus person themselves
            closeFamilyMembers.Add(focusPerson);
            
            // 1. All descendants of the focus person
            var descendants = _familyDAG.GetDescendants(focusPerson.dataBaseOwnerId, 10);
            foreach (var descendant in descendants)
            {
                closeFamilyMembers.Add(descendant);
            }
            
            // 2. Spouse of the focus person
            var spouses = GetSpousesFromDAG(focusPerson);
            foreach (var spouse in spouses)
            {
                closeFamilyMembers.Add(spouse);
            }
            
            // 3. Mother-in-law and Father-in-law (parents of spouse)
            foreach (var spouse in spouses)
            {
                var spouseParents = GetParentsFromDAG(spouse);
                foreach (var spouseParent in spouseParents)
                {
                    closeFamilyMembers.Add(spouseParent);
                }
            }
            
            // 4. Brother and sister "in-laws" (siblings of spouse)
            foreach (var spouse in spouses)
            {
                var spouseSiblings = GetSiblingsFromDAG(spouse);
                foreach (var spouseSibling in spouseSiblings)
                {
                    closeFamilyMembers.Add(spouseSibling);
                }
            }
            
            // 5. All Siblings of the focus person, and their spouses
            var siblings = GetSiblingsFromDAG(focusPerson);
            foreach (var sibling in siblings)
            {
                closeFamilyMembers.Add(sibling);
                var siblingSpouses = GetSpousesFromDAG(sibling);
                foreach (var siblingSpouse in siblingSpouses)
                {
                    closeFamilyMembers.Add(siblingSpouse);
                }
            }
            
            // 6. All Nieces and Nephews (children of siblings)
            foreach (var sibling in siblings)
            {
                var siblingChildren = GetChildrenFromDAG(sibling);
                foreach (var nieceNephew in siblingChildren)
                {
                    closeFamilyMembers.Add(nieceNephew);
                }
            }
            
            // 7. Mother and Father
            var parents = GetParentsFromDAG(focusPerson);
            foreach (var parent in parents)
            {
                closeFamilyMembers.Add(parent);
            }
            
            // 8. Grand Parents
            foreach (var parent in parents)
            {
                var grandParents = GetParentsFromDAG(parent);
                foreach (var grandParent in grandParents)
                {
                    closeFamilyMembers.Add(grandParent);
                }
            }
            
            // 9. Aunts and Uncles (siblings of parents) plus their spouses
            foreach (var parent in parents)
            {
                var parentSiblings = GetSiblingsFromDAG(parent);
                foreach (var auntUncle in parentSiblings)
                {
                    closeFamilyMembers.Add(auntUncle);
                    var auntUncleSpouses = GetSpousesFromDAG(auntUncle);
                    foreach (var auntUncleSpouse in auntUncleSpouses)
                    {
                        closeFamilyMembers.Add(auntUncleSpouse);
                    }
                }
            }
            
            // 10. All Cousins (children of aunts and uncles)
            foreach (var parent in parents)
            {
                var parentSiblings = GetSiblingsFromDAG(parent);
                foreach (var auntUncle in parentSiblings)
                {
                    var cousins = GetChildrenFromDAG(auntUncle);
                    foreach (var cousin in cousins)
                    {
                        closeFamilyMembers.Add(cousin);
                    }
                }
            }
            
            return closeFamilyMembers.ToList();
        }

        /// <summary>
        /// Get siblings of a person using DAG
        /// </summary>
        private List<Person> GetSiblingsFromDAG(Person person)
        {
            var siblings = new List<Person>();
            var parents = GetParentsFromDAG(person);
            
            foreach (var parent in parents)
            {
                var children = GetChildrenFromDAG(parent);
                
                foreach (var child in children)
                {
                    if (child.dataBaseOwnerId != person.dataBaseOwnerId && 
                        !siblings.Any(s => s.dataBaseOwnerId == child.dataBaseOwnerId))
                    {
                        siblings.Add(child);
                    }
                }
            }
            
            return siblings;
        }

        /// <summary>
        /// Get spouses of a person using DAG
        /// </summary>
        private List<Person> GetSpousesFromDAG(Person person)
        {
            var spouses = new List<Person>();
            var directRelationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
            
            foreach (var relationship in directRelationships)
            {
                if (relationship.RelationshipType == PersonRelationshipType.Spouse)
                {
                    var spouse = _familyDAG.People.ContainsKey(relationship.ToPersonId) 
                        ? _familyDAG.People[relationship.ToPersonId] 
                        : null;
                    if (spouse != null)
                    {
                        spouses.Add(spouse);
                    }
                }
            }
            
            return spouses;
        }

        /// <summary>
        /// Get parents of a person using DAG
        /// </summary>
        private List<Person> GetParentsFromDAG(Person person)
        {
            var parents = new List<Person>();
            var directRelationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
            
            foreach (var relationship in directRelationships)
            {
                if ((relationship.RelationshipType == PersonRelationshipType.Father || 
                     relationship.RelationshipType == PersonRelationshipType.Mother) &&
                    relationship.ToPersonId == person.dataBaseOwnerId)
                {
                    var parent = _familyDAG.People.ContainsKey(relationship.FromPersonId) 
                        ? _familyDAG.People[relationship.FromPersonId] 
                        : null;
                    if (parent != null)
                    {
                        parents.Add(parent);
                    }
                }
            }
            
            return parents;
        }

        /// <summary>
        /// Get children of a person using DAG
        /// </summary>
        private List<Person> GetChildrenFromDAG(Person person)
        {
            var children = new List<Person>();
            var directRelationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
            
            foreach (var relationship in directRelationships)
            {
                // Look for Child, Father, or Mother relationships from this person to a child
                if ((relationship.RelationshipType == PersonRelationshipType.Child ||
                     relationship.RelationshipType == PersonRelationshipType.Father ||
                     relationship.RelationshipType == PersonRelationshipType.Mother) &&
                    relationship.FromPersonId == person.dataBaseOwnerId)
                {
                    var child = _familyDAG.People.ContainsKey(relationship.ToPersonId) 
                        ? _familyDAG.People[relationship.ToPersonId] 
                        : null;
                    if (child != null && !children.Any(c => c.dataBaseOwnerId == child.dataBaseOwnerId))
                    {
                        children.Add(child);
                    }
                }
            }
            
            return children;
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
                var parents = GetParentsFromDAG(child);
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
            var marriagesThisYear = new List<(Person bride, Person groom, int marriageYear)>();
            
            // Get all marriages for close family members in this year from DAG
            foreach (var person in closeFamilyMembers)
            {
                var relationships = _familyDAG.GetDirectRelationships(person.dataBaseOwnerId);
                foreach (var relationship in relationships)
                {
                    if (relationship.RelationshipType == PersonRelationshipType.Spouse && 
                        relationship.EventDate.HasValue && 
                        relationship.EventDate.Value == year)
                    {
                        var spouse = _familyDAG.People.ContainsKey(relationship.ToPersonId) 
                            ? _familyDAG.People[relationship.ToPersonId] 
                            : null;
                        
                        if (spouse != null)
                        {
                            // Determine bride and groom based on gender
                            var bride = person.gender == PersonGenderType.Female ? person : spouse;
                            var groom = person.gender == PersonGenderType.Male ? person : spouse;
                            
                            // Avoid duplicates by checking if this marriage is already recorded
                            var existingMarriage = marriagesThisYear.FirstOrDefault(m => 
                                (m.bride.dataBaseOwnerId == bride.dataBaseOwnerId && m.groom.dataBaseOwnerId == groom.dataBaseOwnerId) ||
                                (m.bride.dataBaseOwnerId == groom.dataBaseOwnerId && m.groom.dataBaseOwnerId == bride.dataBaseOwnerId));
                            
                            if (existingMarriage.bride == null)
                            {
                                marriagesThisYear.Add((bride, groom, year));
                            }
                        }
                    }
                }
            }
            
            if (!marriagesThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var marriage in marriagesThisYear)
            {
                var bride = marriage.bride;
                var groom = marriage.groom;
                
                var brideRelationship = GetRelationshipToPerson(bride, focusPerson);
                var groomRelationship = GetRelationshipToPerson(groom, focusPerson);
                
                var brideAge = CalculateAge(bride.originalBirthEventDateYear, marriage.marriageYear);
                var groomAge = CalculateAge(groom.originalBirthEventDateYear, marriage.marriageYear);
                
                sb.AppendLine($"{FormatPersonName(bride)}, {brideAge}, {brideRelationship} of {FormatPersonName(focusPerson)}, was united in marriage to {FormatPersonName(groom)}, {groomAge}, {groomRelationship} of {FormatPersonName(focusPerson)}.");
                sb.AppendLine($"The ceremony took place in {marriage.marriageYear}.");
                
                // Add parents information
                var brideParents = GetParentsFromDAG(bride);
                var groomParents = GetParentsFromDAG(groom);
                
                if (brideParents.Any())
                    sb.AppendLine($"The bride is the daughter of {GetParentsNames(brideParents)}.");
                if (groomParents.Any())
                    sb.AppendLine($"The groom is the son of {GetParentsNames(groomParents)}.");
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Generates death announcements for the given year
        /// </summary>
        private string GenerateDeathAnnouncements(Person focusPerson, List<Person> closeFamilyMembers, int year)
        {
            var sb = new StringBuilder();
            var deathsThisYear = closeFamilyMembers.Where(p => p.originalDeathEventDateYear == year)
                .GroupBy(p => p.dataBaseOwnerId)
                .Select(g => g.First())
                .ToList();
            
            if (!deathsThisYear.Any())
            {
                sb.AppendLine("Nothing to report.");
                return sb.ToString();
            }
            
            foreach (var person in deathsThisYear)
            {
                var relationship = GetRelationshipToPerson(person, focusPerson);
                var age = CalculateAge(person.originalBirthEventDateYear, person.originalDeathEventDateYear);
                
                sb.AppendLine($"{FormatPersonName(person)}, {age}, {relationship} of {FormatPersonName(focusPerson)}, passed away.");
                sb.AppendLine($"Died: {FormatDate(person.originalDeathEventDateMonth, person.originalDeathEventDateDay, person.originalDeathEventDateYear)}");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Gets the relationship between two people using DAG
        /// </summary>
        private string GetRelationshipToPerson(Person relationshipPerson, Person sourcePerson)
        {
            if (_familyDAG != null)
            {
                var relationship = _familyDAG.GetRelationshipBetween(relationshipPerson.dataBaseOwnerId, sourcePerson.dataBaseOwnerId);
                return relationship; // GetRelationshipBetween already returns a human-readable string
            }
            
            return "relative";
        }

        /// <summary>
        /// Formats parent names for announcements
        /// </summary>
        private string GetParentsNames(List<Person> parents)
        {
            if (!parents.Any())
                return "Unknown";
            
            if (parents.Count == 1)
                return FormatPersonName(parents[0]);
            
            var father = parents.FirstOrDefault(p => p.gender == PersonGenderType.Male);
            var mother = parents.FirstOrDefault(p => p.gender == PersonGenderType.Female);
            
            if (father != null && mother != null)
                return $"{FormatPersonName(father)} and {FormatPersonName(mother)}";
            else if (father != null)
                return FormatPersonName(father);
            else if (mother != null)
                return FormatPersonName(mother);
            else
                return string.Join(" and ", parents.Select(p => FormatPersonName(p)));
        }

        /// <summary>
        /// Gets gender text for announcements
        /// </summary>
        private string GetGenderText(PersonGenderType gender)
        {
            return gender == PersonGenderType.Male ? "son" : "daughter";
        }

        /// <summary>
        /// Formats a date for display
        /// </summary>
        private string FormatDate(int month, int day, int year)
        {
            if (month > 0 && day > 0)
                return $"{GetMonthName(month)} {day}, {year}";
            else if (month > 0)
                return $"{GetMonthName(month)} {year}";
            else
                return year.ToString();
        }

        /// <summary>
        /// Gets month name from number
        /// </summary>
        private string GetMonthName(int month)
        {
            string[] months = { "", "January", "February", "March", "April", "May", "June", 
                               "July", "August", "September", "October", "November", "December" };
            return month >= 1 && month <= 12 ? months[month] : "Unknown";
        }

        /// <summary>
        /// Calculates age from birth year to current year
        /// </summary>
        private string CalculateAge(int birthYear, int currentYear)
        {
            if (birthYear <= 0 || currentYear <= 0)
                return "unknown age";
            
            int age = currentYear - birthYear;
            if (age < 0)
                return "unknown age";
            else if (age == 0)
                return "infant";
            else if (age == 1)
                return "1 year old";
            else
                return $"{age} years old";
        }

        /// <summary>
        /// Test DAG functionality
        /// </summary>
        public void TestDAGFunctionality()
        {
            if (_familyDAG == null)
            {
                Debug.LogError("Family DAG is not initialized!");
                return;
            }

            Debug.Log($"DAG contains {_familyDAG.People.Count} people");
            
            // Count total relationships by summing all direct relationships
            int totalRelationships = 0;
            foreach (var person in _familyDAG.People.Values)
            {
                totalRelationships += _familyDAG.GetDirectRelationships(person.dataBaseOwnerId).Count;
            }
            // Divide by 2 since each relationship is counted twice (once for each person)
            totalRelationships /= 2;
            Debug.Log($"DAG contains {totalRelationships} relationships");
        }

        /// <summary>
        /// Get DAG statistics
        /// </summary>
        public string GetDAGStatistics()
        {
            if (_familyDAG == null)
                return "DAG not initialized";

            var sb = new StringBuilder();
            sb.AppendLine($"DAG Statistics:");
            sb.AppendLine($"- People: {_familyDAG.People.Count}");
            
            // Count total relationships by summing all direct relationships
            int totalRelationships = 0;
            foreach (var person in _familyDAG.People.Values)
            {
                totalRelationships += _familyDAG.GetDirectRelationships(person.dataBaseOwnerId).Count;
            }
            // Divide by 2 since each relationship is counted twice (once for each person)
            totalRelationships /= 2;
            sb.AppendLine($"- Relationships: {totalRelationships}");
            
            return sb.ToString();
        }
    }
} 