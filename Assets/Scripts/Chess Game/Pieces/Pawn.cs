using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(PieceCreator))]
public class Pawn : Piece
{
    private PieceCreator pieceCreator;
    private Piece newQueen;

    public override List<Vector2Int> SelectAvaliableSquares()
    {
        throw new NotImplementedException();
    }

    public override void MovePiece(Vector2Int coords)
    {
        if (this.team == TeamColor.White & ((this.occupiedSquare.y != 1 & coords.x - this.occupiedSquare.x == 0 & coords.y - this.occupiedSquare.y == 1) |
            (this.occupiedSquare.y == 1 & coords.x - this.occupiedSquare.x == 0 & coords.y - this.occupiedSquare.y <= 2 & coords.y - this.occupiedSquare.y >= 1)))
        {
            this.occupiedSquare = coords;
            transform.position = this.board.CalculatePositionFromCoords(coords);
            if (this.occupiedSquare.y == 7)
            {
                Destroy(this.gameObject);
                pieceCreator = GetComponent<PieceCreator>();
                string queenName = PieceType.Queen.ToString();
                Type queenType = Type.GetType(queenName);
                newQueen = pieceCreator.CreatePiece(queenType).GetComponent<Piece>();
                //make each piece interactable with AR
                newQueen.gameObject.AddComponent<BoxCollider>();
                newQueen.gameObject.AddComponent<NearInteractionGrabbable>();
                newQueen.gameObject.AddComponent<ObjectManipulator>();

                // add snapping to each piece
                newQueen.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate
                {
                    float distance = board.squareSize * 4;
                    Vector2Int newCoords = new Vector2Int(-1, -1);
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            Vector2Int nextSquare = new Vector2Int(i, j);
                            float newDistance = Vector3.Distance(newQueen.transform.position, board.CalculatePositionFromCoords(nextSquare));
                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                newCoords.Set(i, j);
                            }
                        }
                    }
                    if (distance < board.squareSize * 1.5)
                    {
                        newQueen.MovePiece(newCoords);
                    }
                    else
                    {
                        newQueen.MovePiece(newQueen.occupiedSquare);
                    }
                }
                );

                newQueen.SetData(this.occupiedSquare, this.team, this.board);

                Material teamMaterial = pieceCreator.GetTeamMaterial(this.team);
                newQueen.SetMaterial(teamMaterial);
                if (this.team == TeamColor.Black)
                {
                    newQueen.transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                }
            }
        }
        else if (this.team == TeamColor.Black & ((this.occupiedSquare.y != 6 & coords.x - this.occupiedSquare.x == 0 & this.occupiedSquare.y - coords.y == 1) |
            (this.occupiedSquare.y == 6 & coords.x - this.occupiedSquare.x == 0 & this.occupiedSquare.y - coords.y <= 2 & this.occupiedSquare.y - coords.y >= 1)))
        {
            this.occupiedSquare = coords;
            transform.position = this.board.CalculatePositionFromCoords(coords);
        }
        else
        {
            transform.position = this.board.CalculatePositionFromCoords(this.occupiedSquare);
        }
    }

}