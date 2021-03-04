using System;

namespace BookStore.Identities.Settings
{
  [Serializable]
  public class DatabaseSetting
  {
    public string ConnectionString { get; set; }
    public string Schema { get; set; }
  }
}