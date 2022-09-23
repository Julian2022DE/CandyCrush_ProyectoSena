using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiezasDeJuego : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Board m_Board;

    bool m_isMoving = false;

    public tipodeinterpolacion tipoDeInterpolo;
    public tipodeficha tipoFicha;

    public Vector3 start;
    public Vector3 end;
    public float t;
    public float tiempodemovimiento;
    public bool yaseejecuto;
    //public tipodeficha ficha;
    public tipodeinterpolacion tipointerpolacion;
    public AnimationCurve curve;


    public void SetCord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Init(Board board)
    {
        m_Board = board;
    }
    public void Move(int x, int y, float duracion)
    {
        if (!yaseejecuto)
        {
            StartCoroutine(MoveRoutine(x, y, duracion));
        }
    }

    IEnumerator MoveRoutine(int destX, int destY, float tiempomovimiento)
    {
        Vector2 startPosition = transform.position;
        bool llego = false;
        float tiempotranscurrido = 0;
        yaseejecuto = true;
        while (!llego)
        {
            if (Vector3.Distance(transform.position, new Vector2(destX, destY)) < 0.01f)
            {
                llego = true;
                if (m_Board != null)
                {
                    m_Board.PlaceGamePiece(this, destX, destY);
                }
                break;
            }
            tiempotranscurrido += Time.deltaTime;
        float t = Mathf.Clamp(tiempotranscurrido / tiempodemovimiento, 0f, 1f);

            switch(tipointerpolacion)
            {
                case tipodeinterpolacion.lineal:
                    t = curve.Evaluate(t);
                    break;
                case tipodeinterpolacion.entrada:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                    break;
                case tipodeinterpolacion.salida:
                    t = Mathf.Sin(t * Mathf.PI * .5f);
                    break;
                case tipodeinterpolacion.suavizado:
                    t = t * t * (3 - 2 * t);
                    break;
                case tipodeinterpolacion.massuavizado:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }
            tiempotranscurrido += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, new Vector2(destX, destY), t);
            yield return null;
        }
        m_isMoving = false;
    }

    public enum tipodeinterpolacion
    {
        lineal,
        entrada,
        salida,
        suavizado,
        massuavizado,
    }
    public enum tipodeficha
    {
       Fredy,
       bonnie,
       chica,
       foxy,
       pupet,
       golden,
    }
}



