using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board;

    bool m_isMoving = false;

    public TipoInterpolacion tipoDeInterpolo;
    public TipoFicha tipoFicha;


    internal void SetCoord(int x, int y) // cordenadas x - y
    {
        xIndex = x;
        yIndex = y;
    }

    internal void Init(Board board) // definicion de board dentro de Gamepiece
    {
        m_board = board;
    }

    internal void Move(int x, int y, float moveTime) //declara que es en movimiento
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(x, y, moveTime));
        }
    }


    IEnumerator MoveRoutine(int destX, int destY, float timeToMove) // tipo de casos de los movimientos 
    {
        Vector2 startPosition = transform.position;
        bool reacedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = true;

        while (!reacedDestination)
        {
            if (Vector2.Distance(transform.position, new Vector2(destX, destY)) < 0.01f)
            {
                reacedDestination = true;
                if (m_board != null)
                {
                    m_board.placeGamePiece(this, destX, destY);
                }
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch (tipoDeInterpolo) // casos de fisica pasados a codigo
            {
                case TipoInterpolacion.Lineal:

                    break;

                case TipoInterpolacion.Entrada:

                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);

                    break;

                case TipoInterpolacion.Salida:

                    t = Mathf.Sin(t + Mathf.PI * .5f);

                    break;

                case TipoInterpolacion.Suavizado:

                    t = t * t * (3 - 2 * t);

                    break;

                case TipoInterpolacion.MasSuavizado:

                    t = t * t * t * (t * (t * 6 - 15) + 10);

                    break;
            }

            transform.position = Vector2.Lerp(startPosition, new Vector2(destX, destY), t);

            yield return null;
        }

        m_isMoving = false;
    }

    public enum TipoInterpolacion // Movimiento mas pulido
    {
        Lineal,
        Entrada,
        Salida,
        Suavizado,
        MasSuavizado,
    }

    public enum TipoFicha //diferencia las fichas
    {
        Fredy,
        Bonnie,
        Chica,
        Foxy,
        Puppet,
        Golden,
    }
}