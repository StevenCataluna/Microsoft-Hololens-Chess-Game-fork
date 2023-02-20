using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(PieceCreator))]
public class ChessGameController : MonoBehaviour
{
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    private PieceCreator pieceCreator;
    public int count = 1; // odd = white/ even = black

     private void Awake()
    {
        SetDependencies();
    }

    private void SetDependencies()
    {
        pieceCreator = GetComponent<PieceCreator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartNewGame();
    }

     private void StartNewGame()
    {
        CreatePiecesFromLayout(startingBoardLayout);
    }


    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    private void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        //make each piece interactable with AR
        newPiece.gameObject.AddComponent<BoxCollider>();
        newPiece.gameObject.AddComponent<NearInteractionGrabbable>();
        newPiece.gameObject.AddComponent<ObjectManipulator>();

        // add snapping to each piece
        newPiece.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener ( delegate 
            { 
                float distance = board.squareSize*4;
                Vector2Int newCoords = new Vector2Int(-1, -1);
                for (int i = 0; i<8; i++)
                {
                    for (int j = 0; j<8; j++)
                    {
                        Vector2Int nextSquare = new Vector2Int(i, j);
                        float newDistance = Vector3.Distance (newPiece.transform.position, board.CalculatePositionFromCoords(nextSquare));
                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            newCoords.Set(i,j);
                        }
                    }
                }
                if (distance < board.squareSize*1.5)
                {
                    if (team == TeamColor.White && count%2==1)
                    {
                        newPiece.MovePiece(newCoords);
                        team = TeamColor.Black;
                        count++;    
                    }
                    else if (team == TeamColor.Black && count%2==0)
                    {
                        newPiece.MovePiece(newCoords);
                        team = TeamColor.White;
                        count++;    
                    }
                    else
                    {
                        newPiece.MovePiece(newPiece.occupiedSquare);
                    }
                } else 
                {
                    newPiece.MovePiece(newPiece.occupiedSquare);
                }
            }
        );

        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);
        if (team == TeamColor.Black)
        {
            newPiece.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        }
    }
}
