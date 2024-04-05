
using System.Collections.Generic;

public interface IDeck 
{
    void SetDeck(List<CardData> cards);

    void DrawCard();
}
