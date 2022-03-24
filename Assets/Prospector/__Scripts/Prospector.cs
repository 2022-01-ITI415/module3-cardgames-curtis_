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
			tableau.Add(cp); //Add CardProspector to List<> tableau
			}
		}
	}

}
