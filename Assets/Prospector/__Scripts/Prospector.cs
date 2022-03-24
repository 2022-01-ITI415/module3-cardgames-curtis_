using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;


	[Header("Set Dynamically")]
	public Deck	deck;
	public Layout layout;
	public List<CardProspector> drawPile;
	public Transform layoutAnchor;

	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;
	



	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck>(); //Get the Deck
		deck.InitDeck(deckXML.text); //Pass DeckXML to it
		Deck.Shuffle(ref deck.cards); //This shuffles the deck. The ref keyword passes a reference to deck.cards, which allows
		//deck.cards to be modified by Deck.Shuffle()

		layout = GetComponent<Layout>(); //Get the layout
		layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it

		drawPile = ConvertListCardsToListCardProspectors(deck.cards);
		LayoutGame();


		List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
		{
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach (Card tCD in lCD)
		{
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}
		return (lCP);
		}

		//The Draw function will pull a single card from the drawPile and return it
		CardProspector Draw()
		{
		CardProspector cd = drawPile[0]; //Pull the 0th CardProspector
		drawPile.RemoveAt(0); //Then remove it from List<> drawPile
		return(cd); //And return it
		}

		//LayoutGame() positions the initial tableau of cards, AKA the "mine"
	void LayoutGame()
	{
		//Create an empty GameObject to serve as an anchor for the tableau
		if (layoutAnchor == null)
		{
			GameObject tGO = new GameObject("_LayoutAnchor"); //Create an empty GameObject named _LayoutAnchor in the Hierarchy
			layoutAnchor = tGO.transform; //Grab its transform
			layoutAnchor.transform.position = layoutCenter; //Position it
		}
		CardProspector cp;
		//Follow the layout
		foreach (SlotDef tSD in layout.slotDefs) //Iterate through all the SlotDefs in the layout.slotDefs as tSD
			{
			cp = Draw(); //Pull a card from the top (beginning) of the drawPile
			cp.faceUp = tSD.faceUp; //Set its faceUp to the value in SlotDef
			cp.transform.parent = layoutAnchor; //Make its parent layoutAnchor

			//This replaces the previous parent: deck.deckAnchor, which appears as _Deck in the Hierarchy when the scene is playing
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID); //Set the localPosition of the card based on slotDef
			
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = eCardState.tableau;

			//CardProspectors in the tableau have the state eCardState.tableau
			cp.state = eCardState.tableau; //Set sorting layers
			//cp.SetSortingLayerName(tSD.layerName); //Set the sorting layers
			tableau.Add(cp); //Add CardProspector to List<> tableau
			}

			foreach (CardProspector tCP in tableau)
        	{
				foreach (int hid in tCP.slotDef.hiddenBy)
				{
					cp = FindCardByLayoutID(hid);
					tCP.hiddenBy.Add(cp);
				}
        	}

			// set up target card
			MoveToTarget(Draw());
			// set up draw pile
        	UpdateDrawPile();
		
		}

		CardProspector FindCardByLayoutID(int layoutID)
    	{
        foreach (CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }

        return null;
    	}

		void SetTableauFaces()
    	{
			foreach (CardProspector cd in tableau)
			{
				bool faceUp = true;
				foreach (CardProspector cover in cd.hiddenBy)
				{
					if (cover.state == eCardState.tableau)
					{
						faceUp = false;
					}
				}

				cd.faceUp = faceUp;
			}
   		}

		//move current card to discardpile
		void MoveToDiscard(CardProspector cd)
    	{
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    	}

		//make cd new target card
		void MoveToTarget(CardProspector cd)
    	{
        if(target != null) MoveToDiscard(target);

        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    	}

		//arrange all cards of drawpile to show how muh are left
		 void UpdateDrawPile()
    	{
        CardProspector cd;

			for (int i = 0; i < drawPile.Count; i++)
			{
				cd = drawPile[i];
				cd.transform.parent = layoutAnchor;
				Vector2 dpStagger = layout.drawPile.stagger;
				cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);
				cd.faceUp = false;
				cd.state = eCardState.drawpile;
				cd.SetSortingLayerName(layout.drawPile.layerName);
				cd.SetSortOrder(-10 * i);
			}
    	}
		void CardClicked(CardProspector cd)
    	{
			switch (cd.state)
			{
				case eCardState.target:
					break;

				case eCardState.drawpile:
					MoveToDiscard(target);
					MoveToTarget(Draw());
					UpdateDrawPile();
					break;

				case eCardState.tableau:

				bool validMatch = true;

                if (!cd.faceUp)
                {
                    validMatch = false;
                }

                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }

                if (!validMatch) return;

				tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
					break;
			}
		}

		bool AdjacentRank(CardProspector c0, CardProspector c1)
    		{
			if (!c0.faceUp || !c1.faceUp) return false;

			if (Mathf.Abs(c0.rank - c1.rank) == 1)
			{
				return true;
        	}
			if (c0.rank == 1 && c1.rank == 13) return true;
        	if (c0.rank == 13 && c1.rank == 1) return true;

        	return false;
    		}
		}
}




		


