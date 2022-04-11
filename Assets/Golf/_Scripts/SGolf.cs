using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SGolf : MonoBehaviour
{
    static public SGolf S;

    [Header("Set in Inspector")]
    public TextAsset deckXML2;
    public TextAsset layoutXML_golf;
    public float xOffset = 3;
    public float yOffset = -2 / 5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(.5f, .9f);
    public Vector2 fsPosRun = new Vector2(.5f, .75f);
    public Vector2 fsPosMid2 = new Vector2(.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(.5f, .95f);
    public float reloadDelay = 1f;
    public Text gameOverText, roundResultText;

    [Header("Set Dynamically")]
    public Deck_Golf deck;
    public Layout_Golf layout;
    public List<CardSGolf> drawPile;
    public Transform layoutAnchor;
    public CardSGolf target;
    public List<CardSGolf> tableau;
    public List<CardSGolf> discardPile;

    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {

        deck = GetComponent<Deck_Golf>();
        deck.InitDeck(deckXML2.text);
        Deck_Golf.Shuffle(ref deck.cards);

        layout = GetComponent<Layout_Golf>();
        layout.ReadLayout(layoutXML_golf.text);

        drawPile = ConvertListCardsToListCardSGolf(deck.cards);

        LayoutGame();
    }
    List<CardSGolf> ConvertListCardsToListCardSGolf(List<Card_Golf> lCD)
    {
        List<CardSGolf> lCP_Golf = new List<CardSGolf>();
        CardSGolf tCP_Golf;
        foreach (Card_Golf tCD_Golf in lCD)
        {
            tCP_Golf = tCD_Golf as CardSGolf;
            lCP_Golf.Add(tCP_Golf);
        }
        return (lCP_Golf);
    }

    CardSGolf Draw()
    {

        CardSGolf cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardSGolf cp;
        foreach (SlotDef_Golf tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState_Golf.tableau;
            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }
        foreach (CardSGolf tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    CardSGolf FindCardByLayoutID(int layoutID)
    {
        foreach (CardSGolf tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
            {
                return (tCP);
            }
        }
        return (null);
    }

    void SetTableauFaces()
    {
        foreach (CardSGolf cd in tableau)
        {
            bool faceUp = true;
            foreach (CardSGolf cover in cd.hiddenBy)
            {
                if (cover.state == eCardState_Golf.tableau)
                {
                    faceUp = true;
                }
            }
            cd.faceUp = faceUp;
        }
    }

    void MoveToDiscard(CardSGolf cd)
    {
        cd.state = eCardState_Golf.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + .5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToTarget(CardSGolf cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState_Golf.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        CardSGolf cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + .1f * i);
            cd.faceUp = false;
            cd.state = eCardState_Golf.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardSGolf cd)
    {
        switch (cd.state)
        {
            case eCardState_Golf.target:
                break;
            case eCardState_Golf.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();

                break;
            case eCardState_Golf.tableau:
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
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }
        if (drawPile.Count > 0)
        {
            return;
        }
        foreach (CardSGolf cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                return;
            }
        }
        GameOver(false);
    }

    void GameOver(bool won)
    {

        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!";
            ShowResultsUI(true);
            //print("Game Over. You Won! :");

        }
        else
        {
            gameOverText.text = "Game Over";
            ShowResultsUI(true);
            //print("Game Over. You Lost. :");

        }
        SceneManager.LoadScene("mainScene");
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene("mainScene");
    }

    public bool AdjacentRank(CardSGolf c0, CardSGolf c1)
    {
        if (!c0.faceUp || !c1.faceUp) return (false);

        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);
        return (false);
    }
}