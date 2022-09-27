using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Pantallas : MonoBehaviour
{
    public void Nivel_1() //carga la scena 1
    {
        SceneManager.LoadScene("LVL 1");
    } 
    public void Nivel_2() //carga la scena 2
    {
        SceneManager.LoadScene("LVL 2");
    }
    public void Nivel_3() //carga la scena 3
    {
        SceneManager.LoadScene("LVL 3");
    } 
    public void Nivel_4() //carga la scena 4
    {
        SceneManager.LoadScene("LVL 4");
    }
    public void Nivel_5() //carga la scena 5
    {
        SceneManager.LoadScene("LVL 5");
    }
    public void Menu()
    {
        SceneManager.LoadScene("Pnatlla inicial");
    }
}
