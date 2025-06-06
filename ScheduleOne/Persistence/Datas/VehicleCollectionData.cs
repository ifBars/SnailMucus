namespace ScheduleOne.Persistence.Datas
{
	public class VehicleCollectionData : SaveData
	{
		public VehicleData[] Vehicles;

		public VehicleCollectionData(VehicleData[] vehicles)
		{
			Vehicles = vehicles;
		}
	}
}
