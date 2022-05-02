using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject cardTemplate, GameController;
    private List<Deck.Card> cardsInStock = new List<Deck.Card>();
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private int cardSpacing = 50;
    [SerializeField] private float handOffsetY = 540f;
    [SerializeField] private float handOffsetX = 150f;

    private List<GameObject> cardObjects = new List<GameObject>();

    private int lastInventoryCount = 0;

    // Adds card to inventory to be rendered
    public void AddToInventory(Deck.Card newCard, GameObject player)
    {
        cardsInStock.Add(newCard);
        UpdateInventory(player);
    }

    public void RemoveFromInventory(Deck.Card card, GameObject player)
    {
        cardsInStock.Remove(card);
        UpdateInventory(player);
    }

    public void Start()
    {
        // AddTestCard();
        // AddTestCard();
        // AddTestCard();
    }

    /*public void AddTestCard()
    {
        AddToInventory(
             new Deck.Card(
                 "Angel's Head", // name 
                 "Head",  // body part
                 "Rare", //rarirty 
                 0, //health
                 0, // atk
                 0, //speed
                 "You've got someone watching over you", //flavor text
                 "Get rid of two body part curses", // gather ability
                 "Get a random curse and apply it to the opponent. Accuracy : 10", // attack ability
                 "None", // curse
                 "Divine", //set
                 defaultSprite, // image
                 "None" //gesture
            )
        );
    }*/

    private void UpdateInventory(GameObject player)
    {
        //if (cardObjects.Count == 0 || lastInventoryCount == cardObjects.Count)
        //{
            //return;
        //}
        lastInventoryCount = cardObjects.Count;
        // Clean up old card objects
        cardObjects.ForEach(delegate (GameObject cardObj)
        {
            Destroy(cardObj.gameObject);
        });
        cardObjects.Clear();
        if (GameController.GetComponent<GameController>() != null)
        {
            if (player == GameController.GetComponent<GameController>().player1)
            {
                DataStorage.player1Inventory = cardsInStock;
            }
            else if (player == GameController.GetComponent<GameController>().player2)
            {
                DataStorage.player2Inventory = cardsInStock;
            }
        }
        else if (GameController.GetComponent<GearUpGameController>() != null)
        {
            if (player == GameController.GetComponent<GearUpGameController>().player1)
            {
                DataStorage.player1Inventory = cardsInStock;
            }
            else if (player == GameController.GetComponent<GearUpGameController>().player2)
            {
                DataStorage.player2Inventory = cardsInStock;
            }
        }
        // Add new card object based on the current inventory
        for (int i = 0; i < cardsInStock.Count; i++)
        {
            Deck.Card currentCard = cardsInStock[i];
            cardObjects.Add(Instantiate(
                cardTemplate,
                this.gameObject.transform
            ));
            //cardObjects[i].gameObject.transform.position = new Vector3(handOffsetX, handOffsetY + cardSpacing * i, 0);
            cardObjects[i].GetComponent<InventoryCard>().SetCardRef(currentCard, player);
        }
    }

    private void Update()
    {
        //UpdateInventory();
        // Debug.Log(cardsInStock.Count);
    }
}