using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiezasDeJuego : MonoBehaviour
{
    public int cordenadax;
    public int cordenaday;

    public Vector3 start;
    public Vector3 end;
    public float t;
    public float tiempodemovimiento;
    public bool yaseejecuto;
    public tipodeinterpolacion tipointerpolacion;

    private void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Movepieces(new Vector3((int)transform.position.x, (int)transform.position.y + 1, 0), tiempodemovimiento);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Movepieces(new Vector3((int)transform.position.x, (int)transform.position.y - 1, 0), tiempodemovimiento);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Movepieces(new Vector3((int)transform.position.x +1, (int)transform.position.y, 0), tiempodemovimiento);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Movepieces(new Vector3((int)transform.position.x -1, (int)transform.position.y, 0), tiempodemovimiento);
        }
    }
    public void Cordenadas(int x, int y)
    {
        cordenadax = x;
        cordenaday = y;
    }

    void Movepieces(Vector3 finaldelpunto, float tiempomovimiento)
    {
        if (yaseejecuto == true) 
        {
            StartCoroutine(Movepiece(finaldelpunto, tiempomovimiento));
        }
    }

    IEnumerator Movepiece(Vector3 finaldelpunto, float tiempomovimiento)
    {
        yaseejecuto = false;

        bool llego = false;
        float tiempotranscurrido = 0;
        Vector3 startPosition = transform.position;
        while (!llego)
        {
            if (Vector3.Distance(transform.position, finaldelpunto) < 0.01f)
            {
                llego = true;
                yaseejecuto = true;
                transform.position = new Vector3((int)finaldelpunto.x, (int)startPosition.y, 0);
                break;
            }
            float t = tiempotranscurrido / tiempomovimiento;

            switch(tipointerpolacion)
            {
                case tipodeinterpolacion.lineal:
                    break;
                case tipodeinterpolacion.entrada:
                    break;
                case tipodeinterpolacion.salida:
                    break;
                case tipodeinterpolacion.suavizado:
                    break;
                case tipodeinterpolacion.massuavizado:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;




            }
            t = t * t * (3 - 2 * t);
            tiempotranscurrido += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, finaldelpunto, t);
            yield return new WaitForEndOfFrame();
        }
    }

    public enum tipodeinterpolacion
    {
        lineal,
        entrada,
        salida,
        suavizado,
        massuavizado,
    }
}



