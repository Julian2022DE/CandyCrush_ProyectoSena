using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    AudioSource source;
    public AudioClip audioFX;

    Board m_board;

    
    public void Init(int cambioX, int cambioY, Board board) // declara los vectores x - y y board 
    {
        xIndex = cambioX;
        yIndex = cambioY;
        m_board = board;
    }
    
    public void OnMouseDown() // agarra el tile inicial 
    {
        m_board.ClickedTile(this);
    }

    public void OnMouseEnter() //mantiene selecionado el tile
    {
        m_board.DragToTile(this);
    }

    public void OnMouseUp() //suelta el tile
    {
        m_board.ReleaseTile();
        AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position);
    }

}
