namespace MFRC522
{
    public interface ICardReader
    {
        bool IsCardAvailable();
        byte[] GetCardId();   
    }
}