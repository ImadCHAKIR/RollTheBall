using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerScript : MonoBehaviour
{
    public float speed = 10;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI midScreenText;
    public GameObject Key;
    public GameObject LeftDoor, RightDoor;
    public int pickUpNumber;
    public GameObject endpoint;
    public int life = 3;
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    public AudioClip coinAudio;
    public AudioClip keyAudio;


    private Vector3 offset;
    private Vector3 finalOffset, finalOffsetY;
    private float sideOffset = .25f, adaptedInputMouseY;
    private bool zoomed;
    private bool toggleSideOffset;
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    private List<String> col;
    private static int score = 0;
    private Vector3 initialPosition;
    private Camera cam;
    private AudioSource audioSource;
    

    #region Unity callbacks
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        midScreenText.gameObject.SetActive(false);
        Key.gameObject.SetActive(false);
        col = new List<String>();
        initialPosition= transform.position;
        setScoreText();
        cam = GetComponentInChildren<Camera>();
        audioSource = GetComponent<AudioSource>();
        
        offset = cam.transform.position - transform.position;
        finalOffset = offset;
        finalOffsetY = offset;
        zoomed = false;
        toggleSideOffset = false;
        finalOffset.x += sideOffset;
        Cursor.visible = false;
    }

    private void Update()
    {
        //Environnement
        OpenDoor();
        NextLevel();

        //Player
        OnDead();

        //Camera
        ToggleSideOffset();
        Zoom();
    }

    private void LateUpdate()
    {
        movementX = Input.GetAxisRaw("Horizontal");
        movementY = Input.GetAxisRaw("Vertical");

        //Player
        transform.Rotate(0f, Input.GetAxis("Mouse X") * 4f, 0f);
        transform.position = transform.position + new Vector3(cam.transform.forward.x,0f,cam.transform.forward.z) * movementY * speed * Time.deltaTime;
        transform.position = transform.position + new Vector3(cam.transform.forward.z, 0f, -cam.transform.forward.x) * movementX * speed * Time.deltaTime;

        //Camera
        Rotate();
        cam.transform.position = transform.position + finalOffset;
        cam.transform.LookAt(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (audioSource != null)
        {
            audioSource.Play(0);
        }

        if (other.gameObject.CompareTag("PickUp"))
        {
            audioSource.PlayOneShot(coinAudio,1);
            other.gameObject.SetActive(false);
            score += 1;

            if (score%pickUpNumber == 0)
            {
                Key.SetActive(true);
            }
            setScoreText();
        }

        if (other.gameObject.CompareTag("Key"))
        {
            audioSource.PlayOneShot(keyAudio, 1);
            Destroy(Key);
            midScreenText.gameObject.SetActive(true);
            Invoke("hideMessage", 2);
        }
    }
    #endregion

    #region Environnement
    private void NextLevel()
    {
        if (transform.position.z > endpoint.transform.position.z && Key.IsDestroyed())
        {
            SceneManager.LoadScene("Level", LoadSceneMode.Single);
            initialPosition = transform.position;
        }

    }

    private void OpenDoor()
    {
        if (Key.IsDestroyed() && LeftDoor.transform.rotation.y < 0.7)
        {
            LeftDoor.transform.position += new Vector3(1 / 100f, 0f, 1 / 100f);
            LeftDoor.transform.Rotate(new Vector3(0f, 0.9f, 0f));

            RightDoor.transform.position += new Vector3(-1 / 100f, 0f, 1 / 100f);
            RightDoor.transform.Rotate(new Vector3(0f, -0.9f, 0f));
        }
    }
    #endregion

    #region Player
    private void OnDead()
    {
        if (transform.position.y < -100 && life >= 0)
        {
            transform.position = initialPosition;
            life -= 1;
            if (life == -1)
            {
                midScreenText.text = "You Lost";
                midScreenText.gameObject.SetActive(true);
                Invoke("hideMessage", 2);
                Invoke("restartGame", 2);
            }
        }
    }
    #endregion

    #region Camera
    private void Rotate()
    {
        adaptedInputMouseY = -Input.GetAxis("Mouse Y");  
        if (adaptedInputMouseY < 0)
        {
            if (cam.transform.eulerAngles.x < 360f && 180f < cam.transform.eulerAngles.x)
                adaptedInputMouseY = 0;
            else if (cam.transform.eulerAngles.x < 135f && 90f < cam.transform.eulerAngles.x)
                adaptedInputMouseY = 0;
        }

        else if (adaptedInputMouseY > 0)
        {
            if (cam.transform.eulerAngles.x > 45f && 90f > cam.transform.eulerAngles.x)
                adaptedInputMouseY = 0;
            else if (cam.transform.eulerAngles.x > 180f && 270f > cam.transform.eulerAngles.x)
                adaptedInputMouseY = 0;
        }
     
        finalOffset = Quaternion.AngleAxis(adaptedInputMouseY * 4f, new Vector3(cam.transform.forward.z, 0f, -cam.transform.forward.x)) * Quaternion.AngleAxis(Input.GetAxis("Mouse X") * 4f, Vector3.up) * finalOffset;
    }

    private void Zoom()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (zoomed)
            {
                zoomed = false;
                finalOffset.y = offset.y;
                finalOffset.z = offset.z;

            }
            else
            {
                zoomed = true;
                finalOffset.y = offset.y / 2;
                finalOffset.z = offset.z / 2;
            }
        }
    }

    private void ToggleSideOffset()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (toggleSideOffset)
            {
                toggleSideOffset = false;
                finalOffset.x -= (2 * sideOffset);
            }
            else
            {
                toggleSideOffset = true;
                finalOffset.x += (2 * sideOffset);
            }
        }
    }
    #endregion

    #region textMesh
    void setScoreText()
    {
        scoreText.SetText("Score: " + score.ToString());
    }

    void hideMessage()
    {
        midScreenText.gameObject.SetActive(false);
    }

    #endregion

    #region general
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void restartGame()
    {
        SceneManager.LoadScene("Level", LoadSceneMode.Single);
    }
    #endregion
}
