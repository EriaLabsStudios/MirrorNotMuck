using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    
    public Action<int> OnBulletsUpdated;
    public Action<int> OnHealthUpdated;
    public Action<int> OnPointsUpdated;
    public TextMeshProUGUI bulletsText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI pointsText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateBullets(int bullets)
    {
        bulletsText.text = "Bullets: " + bullets;
    }

    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health;
    }

    public void UpdatePoints(int points)
    {
        pointsText.text = "Points: " + points;
    }
}
