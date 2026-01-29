namespace Internal.Scripts.Save
{
    public interface ISaveService
    {
        bool HasSave();
        SaveData Load();
        void Save(SaveData data);
    }
}
