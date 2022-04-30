using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Leap;
using Leap.Unity;
using UnityEngine.SceneManagement;

public class GameControllerDemo : MonoBehaviour
{
    [SerializeField] private LeapServiceProvider leapController;
    [SerializeField] private UnityEngine.UI.Image backgroundShader;
    [SerializeField] private TMP_Text TurnText;
    [SerializeField] private GameObject gridHolder, dice, canvasRotator, cardController;
    [SerializeField] private Camera cameraMain;
    [HideInInspector] public Vector3 camPosMain;
    [HideInInspector] public Color movementColor = new Color(1f, 1f, 1f, 0.5f);

    // controls checking for hand dice roll motion and the motion steps for it.
    private bool checkForHandRoll = false;
    private bool checkForHandRollMoveOne = false;

    public List<GameObject> rangeHexes = new List<GameObject>();
    public GameObject currentPlayer, player1, player2, rollButton;
    private bool diceRolled = false;
    public int numMounds, numGraves, numMausoleums;
    [SerializeField] private int numTurnsDemo;
    [SerializeField] private int numTurnsRegular;

    public int currentPlayerNum = 1;

    public int currentTurnNum;
    public Text startText; // used for showing countdown from 3, 2, 1 

    // get reference to player menus to disable / enable pinch motion.
    [SerializeField] private PlayerMenu p1Menu;
    [SerializeField] private PlayerMenu p2Menu;

    // reference to first hand being tracked in frame.
    private int firstHandID;

    //Start the game with player 1 rolling to move
    void Start()
    {
        camPosMain = cameraMain.transform.position;
        currentPlayer = player1;
        StartCoroutine(PlaceDigsites());
    }

    //This uses a raycast from the camera to the mouse pointers position to determine where it is clicking. If it is clicking a tile that is
    //lit up with the movement color then the player moves to that tile and it starts the other players turn
    void Update()
    {
        //The combat scene is loaded after each player gets five turns - the currentTurnNum goes up by 1 every time a player moves so 5 x 2 = 10
        if (currentTurnNum == 20)
        {
            //startText.text = currentTurnNum.ToString();
            SceneManager.LoadScene("CombatPhase");
            Debug.Log("Each player got 10 turns in demo mode so scene was switched to combat phase");
        }

        //The combat scene is loaded after each player gets ten turns - the currentTurnNum goes up by 1 every time a player moves so 10 x 2 = 20
        // if (currentTurnNum == 20)
        // {
        //     SceneManager.LoadScene("CombatPhase");
        //     Debug.Log("Each player got 5 turns in demo mode so scene was switched to combat phase");
        // }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider != null && hit.collider.gameObject.GetComponent<SpriteRenderer>().color == movementColor)
            {
                StartCoroutine(movement(hit.collider.gameObject));
            }
        }

        // having both hands enter at the same time is unlikely but doable and causes an object reference error, so need to try catch this.
        try
        {
            // if a leap service provider is connected and a hand is being tracked, 
            // get an ID reference to the first entered tracked hand, check for conditions. 
            // if two hands are tracked, just watch for conditions on the first entered hand.
            if (leapController && leapController.CurrentFrame.Hands.Count == 1)
            {
                firstHandID = leapController.CurrentFrame.Hands[0].Id;
                HandDiceRoll(leapController.CurrentFrame.Hand(firstHandID));
            }
            else if (leapController && leapController.CurrentFrame.Hands.Count == 2)
            {
                HandDiceRoll(leapController.CurrentFrame.Hand(firstHandID));
            }
        }
        // if both hands entered at the same time, just track hand 0. when two hands are being tracked, it's the left hand.
        catch (Exception e)
        {
            if (leapController && leapController.CurrentFrame.Hands.Count > 0)
            {
                Hand hand = leapController.CurrentFrame.Hands[0];
                HandDiceRoll(hand);
            }
        }
    }

    //This is where caling the DiceRoll function returns. Currently it only has functionality for moving
    public void RollResult(int result, string type)
    {
        StartCoroutine(ZoomOut());
        switch (type)
        {
            case "move":
                currentPlayer.GetComponent<PlayerMovement>().MoveArea(result);
                break;
            case "attack":
                break;
        }
    }

    //implement a way to end turns by running the TurnStart coroutine five times for demo?
    public IEnumerator TurnEndDemo()
    {
        //add turnstart coroutine five times to make five turns for demo
        yield return new WaitForSeconds(1.0f);
        Debug.Log("turn end");
    }

    //implement a way to end turns by running the TurnStart coroutine five times for regular version?
    public IEnumerator TurnEndRegular()
    {
        //add turnstart coroutine ten times to make ten turns for regular version
        yield return new WaitForSeconds(1.0f);
        Debug.Log("turn end");
    }

    //Coroutine that controls everything that happnes at the begining of the turn with rolling for movement and displaying whose turn it is
    public IEnumerator TurnStart(int playerNum)
    {
        currentPlayerNum = currentPlayerNum == 1 ? 2 : 1;

        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / 15f;

            if (t > 1)
            {
                t = 1;
            }

            cameraMain.transform.position = Vector3.Lerp(cameraMain.transform.position, new Vector3(currentPlayer.transform.position.x, currentPlayer.transform.position.y, -5), t);
            if (cameraMain.transform.position.z > -5.01f)
            {
                break;
            }
            yield return null;
        }

        cameraMain.transform.position = new Vector3(currentPlayer.transform.position.x, currentPlayer.transform.position.y, -5);

        if (playerNum == 1)
        {
            canvasRotator.transform.localRotation = Quaternion.Euler(0, 0, -90);
            TurnText.text = "Player 1's Turn";
            currentTurnNum++;
            Debug.Log("Player one has had: " + currentTurnNum + " turns. Scene will switch once # reaches 10 for both players.");
            startText.text = currentTurnNum.ToString();
        }
        else if (playerNum == 2)
        {
            canvasRotator.transform.localRotation = Quaternion.Euler(0, 0, 90);
            TurnText.text = "Player 2's Turn";
            currentTurnNum++;
            Debug.Log("Player one has had: " + currentTurnNum + " turns. Scene will switch once # reaches 10 for both players.");
            startText.text = currentTurnNum.ToString();
        }
        float a = 0f;
        while (a < 0.785)
        {
            a += 0.004f;
            backgroundShader.color = new Color(0f, 0f, 0f, a);
            TurnText.color = new Color(1f, 1f, 1f, a + 0.215f);
            yield return new WaitForSeconds(0.0025f);
        }

        // check for button click or hand motion.
        rollButton.SetActive(true);
        checkForHandRoll = true;

        // disable pinch motion. 
        p1Menu.useHandMotion = false;
        p2Menu.useHandMotion = false;

        while (diceRolled == false)
        {
            yield return null;
        }
        // stop checking for hand motion. 
        checkForHandRoll = false;
        // reset hand motion steps if interrupted.
        checkForHandRollMoveOne = false;
        checkForHandRoll = false;
        // re-enable pinch motion. 
        p1Menu.useHandMotion = true;
        p2Menu.useHandMotion = true;
        rollButton.SetActive(false);
        dice.GetComponent<DiceScript>().DiceRoll(8, "move");
        diceRolled = false;
        yield return new WaitForSeconds(7f);
        while (a > 0)
        {
            a -= 0.004f;
            backgroundShader.color = new Color(0f, 0f, 0f, a);
            TurnText.color = new Color(1f, 1f, 1f, a);
            yield return new WaitForSeconds(0.0025f);
        }
        TurnText.color = new Color(1f, 1f, 1f, 0f);
    }

    public void RollDice()
    {
        diceRolled = true;
    }

    private void HandDiceRoll(Hand hand)
    {
        // don't check when not waiting for a hand roll.
        if (!checkForHandRoll) return;

        // get fingers.
        Finger thumb = hand.Fingers[0];
        Finger index = hand.Fingers[1];
        Finger middle = hand.Fingers[2];
        Finger ring = hand.Fingers[3];
        Finger pinky = hand.Fingers[4];

        if (!thumb.IsExtended
            && !index.IsExtended
            && !middle.IsExtended
            && !ring.IsExtended
            && !pinky.IsExtended
            && !checkForHandRollMoveOne
        )
        {
            // put some visual indicator for these steps?
            Debug.Log("hand is closed");
            // hand is closed.
            checkForHandRollMoveOne = true;
        }
        if (thumb.IsExtended
            && index.IsExtended
            && middle.IsExtended
            && ring.IsExtended
            && pinky.IsExtended
            && checkForHandRollMoveOne
        )
        {
            Debug.Log("hand is opened, dice rolled");
            // hand is opened, dice is rolled.
            checkForHandRollMoveOne = false;
            checkForHandRoll = false;
            diceRolled = true;
        }
    }

    public void HandFingerGun(Hand hand)
    {
        // don't check when not waiting for a hand roll.
        if (!checkForHandRoll) return;

        // get fingers.
        Finger thumb = hand.Fingers[0];
        Finger index = hand.Fingers[1];
        Finger middle = hand.Fingers[2];
        Finger ring = hand.Fingers[3];
        Finger pinky = hand.Fingers[4];

        if (!thumb.IsExtended
            && !index.IsExtended
            && !middle.IsExtended
            && !ring.IsExtended
            && !pinky.IsExtended
            && !checkForHandRollMoveOne
        )
        {
            // put some visual indicator for these steps?
            Debug.Log("hand is closed");
            // hand is closed.
            checkForHandRollMoveOne = true;
        }
        if (thumb.IsExtended
            && index.IsExtended
            && middle.IsExtended
        )
        {
            Debug.Log("finger guns");
            // hand is opened, dice is rolled.
            checkForHandRollMoveOne = false;
            checkForHandRoll = false;
            diceRolled = true;
        }
    }

    //Coroutine that places all the gravesites one by one at the beginning of the game
    IEnumerator PlaceDigsites()
    {
        int i = 0;
        float interval = 0.05f;
        //Number of mounds
        while (i <= numMounds)
        {
            int site = UnityEngine.Random.Range(0, gridHolder.transform.childCount);
            if (gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count < 5)
            {
                continue;
            }
            bool alreadyAssigned = false;
            for (int j = 0; j < gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count; j++)
            {
                List<ArrayList> hexList = gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes;
                GameObject hex = (GameObject)hexList[j][0];
                if (hex.GetComponent<HexScript>().type != "")
                {
                    alreadyAssigned = true;
                }
            }
            if (alreadyAssigned == true)
            {
                continue;
            }
            gridHolder.transform.GetChild(site).GetComponent<HexScript>().MoundHex();
            i++;
            yield return new WaitForSeconds(interval);
        }
        i = 0;
        //number of graves
        while (i <= numGraves)
        {
            int site = UnityEngine.Random.Range(0, gridHolder.transform.childCount);
            if (gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count < 6)
            {
                continue;
            }
            bool alreadyAssigned = false;
            for (int j = 0; j < gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count; j++)
            {
                List<ArrayList> hexList = gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes;
                GameObject hex = (GameObject)hexList[j][0];
                if (hex.GetComponent<HexScript>().type != "")
                {
                    alreadyAssigned = true;
                }
            }
            if (alreadyAssigned == true)
            {
                continue;
            }
            gridHolder.transform.GetChild(site).GetComponent<HexScript>().GraveHex();
            i++;
            yield return new WaitForSeconds(interval);
        }
        i = 0;
        //number of mausoleums
        while (i <= numMausoleums)
        {
            int site = UnityEngine.Random.Range(0, gridHolder.transform.childCount);
            if (gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count < 6)
            {
                continue;
            }
            bool alreadyAssigned = false;
            for (int j = 0; j < gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes.Count; j++)
            {
                List<ArrayList> hexList = gridHolder.transform.GetChild(site).GetComponent<HexScript>().nearHexes;
                GameObject hex = (GameObject)hexList[j][0];
                if (hex.GetComponent<HexScript>().type != "")
                {
                    alreadyAssigned = true;
                }
            }
            if (alreadyAssigned == true)
            {
                continue;
            }
            gridHolder.transform.GetChild(site).GetComponent<HexScript>().MausoleumHex();
            i++;
            yield return new WaitForSeconds(interval);
        }
        StartCoroutine(TurnStart(1));
    }

    //This is a corourtine that controls the camera zooming out after the dice is rolled for movement
    IEnumerator ZoomOut()
    {
        float t = 0f;
        Vector3 currentCameraPos = cameraMain.transform.position;
        while (t < 1)
        {
            t += Time.deltaTime / 0.25f;

            if (t > 1)
            {
                t = 1;
            }

            cameraMain.transform.position = Vector3.Lerp(currentCameraPos, new Vector3(camPosMain[0], camPosMain[1], camPosMain[2]), t);
            if (cameraMain.transform.position.z < camPosMain[2] + 0.01f)
            {
                break;
            }
            yield return null;
        }

        cameraMain.transform.position = new Vector3(camPosMain[0], camPosMain[1], camPosMain[2]);
    }

    //Coroutine that takes care of the smooth movement as well as checks if they move to a gravesite
    IEnumerator movement(GameObject destination)
    {
        //revert the movement tiles
        for (int i = 0; i < rangeHexes.Count; i++)
        {
            //Revert(1) will revert the last child which is reserved for the movement and are lighting up
            rangeHexes[i].GetComponent<HexScript>().Revert(1);
        }

        //Checks to see if the player is at their destination or not
        while (currentPlayer.transform.position.x >= destination.transform.position.x + 0.01f || currentPlayer.transform.position.x <= destination.transform.position.x - 0.01f ||
            currentPlayer.transform.position.y >= destination.transform.position.y + 0.01f || currentPlayer.transform.position.y <= destination.transform.position.y - 0.01f)
        {
            currentPlayer.transform.position = Vector3.Lerp(currentPlayer.transform.position, destination.transform.position, Time.deltaTime * 8f);
            yield return null;
        }
        currentPlayer.transform.position = destination.transform.position;

        string tileType = destination.transform.parent.GetComponent<HexScript>().type;

        //Breaks coroutine if they land on a gravesite
        if (tileType == "Mound" || tileType == "Grave" || tileType == "Mausoleum")
        {
            StartCoroutine(cardController.GetComponent<CardController>().digging(tileType));
            yield break;
        }

        currentPlayer.GetComponent<PlayerMovement>().FindTile();
        if (currentPlayer == player1) { currentPlayer = player2; StartCoroutine(TurnStart(2)); }
        else { currentPlayer = player1; StartCoroutine(TurnStart(1)); }
    }
}
