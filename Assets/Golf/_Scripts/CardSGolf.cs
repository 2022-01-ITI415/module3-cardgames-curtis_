using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState_Golf
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardSGolf : Card_Golf
{
    [Header("Set Dynamically: CardSGolf")]
    public eCardState_Golf state = eCardState_Golf.drawpile;
    public List<CardSGolf> hiddenBy = new List<CardSGolf>();
    public int layoutID;
    public SlotDef_Golf slotDef;

    public override void OnMouseUpAsButton()
    {
        SGolf.S.CardClicked(this); 
        base.OnMouseUpAsButton();
    }

}
