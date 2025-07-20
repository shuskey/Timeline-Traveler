namespace Assets.Scripts.Enums
{
    public enum PersonRelationshipType
    {
        NotSet,
        Mother,
        Father,
        Child,
        Spouse,
        
        // Extended family relationships for DAG
        Sibling,
        GrandParent,
        GrandChild,
        GreatGrandParent,
        GreatGrandChild,
        AuntUncle,
        NieceNephew,
        Cousin,
        InLaw,
        
        // For step relationships
        StepParent,
        StepChild,
        StepSibling,
        HalfSibling,
        
        // For more distant relationships
        SecondCousin,
        ThirdCousin,
        Relative // Generic catchall
    }
}
