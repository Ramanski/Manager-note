using CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mannote
{
    class LogicEditor
    {
        // Создать объект контекста
        static private SampleContext context = new SampleContext();
        static private List<Cargo> cargos = new List<Cargo>();
        static public int powerKind { get; set; }
        static public int trainType { get; set; }
        static private IQueryable<Lokomotive> freeLokomotives;
        static private IEnumerable<OperationsView> actualTrains; 

        public static void ConnectData()
        {
            try
            {
                InitializeQueries();
            }
            catch(Exception)
            {
                throw new Exception();
            }
            // !!Построение БД заново, если модель изменилась!!
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SampleContext>());
            //Логгирование запросов к БД
            context.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
        }

        //с помощью LINQ
        private static void InitializeQueries()
        {
            //Запрос свободных локомотивов со статусом "брошен" или "прибыл"
            freeLokomotives = context.Lokomotives
                                     .Where(l => (l.Code.CodeId == 204 || l.Code.CodeId == 200) &&
                                            l.PowerKind.PowerKindId == powerKind &&
                                            l.TrainType.TrainTypeId == trainType);
            //Выборка сведений по всем поездам, кроме расформированных.
            actualTrains = context.Trains
                .Where(t => t.Operations
                            .OrderByDescending(o => o.OperationId)
                            .FirstOrDefault()
                            .Code.CodeId != 205)
                .Select(t => new
                {
                    num = t.TrainId,
                    stot = t.Path.DepartureStation.Name,
                    stnz = t.Path.ArriveStation.Name,
                    oper = t.Operations.FirstOrDefault().Code.Name,
                    time = t.Operations.FirstOrDefault().Date,
                    train = t
                })
                .AsEnumerable()
                .Select(an => new OperationsView
                {
                    stot = an.stot,
                    stnz = an.stnz,
                    oper = an.oper,
                    time = an.time,
                    train = an.train
                });
        }

        private static Path FindPath(Station DepartureStation, Station ArrivalStation)
        {
            string ErrorText = "Произошла непредвиденная ситуация";
            try
            {
                int stot = DepartureStation.StationId;
                int stnz = ArrivalStation.StationId;
                //Поиск пути между выбранными станциями
                return context.Paths.Where(p => p.DepartureStation.StationId == stot
                                                 && p.ArriveStation.StationId == stnz
                                                 ).Single();
            }
            catch (ArgumentNullException)
            {
                ErrorText = "Не удалось найти маршрут по выбанным станциям";
            }
            catch (InvalidOperationException)
            {
                ErrorText = "Найдено несколько вариантов маршрута по выбанным станциям";
            }
            catch (Exception)
            {
                throw new Exception(ErrorText);
            }
            return null;
        }

        private static float ProcessCargoes(Train train)
        {
            float Weight = 0;
            foreach (Cargo cargo in cargos)
            {
                cargo.Train = train;
                cargo.CostToTransport *= train.Path.Distance;
                Weight += cargo.Weight;
            }
            return Weight;
        }

        // Сохранить изменения в БД
        private static void TrySaveChanges()
        {
            string ErrorText = "Произошла непредвиденная ситуация";
            try
            {
                context.SaveChanges();
                }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                ErrorText = "Произошла ошибка при обновлении записей.";
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException)
            {
                ErrorText = "Произошла ошибка при валидации полученных данных.";
            }
            catch (InvalidOperationException)
            {
                ErrorText = "Произошла ошибка записи в базу данных.";
            }
            catch (Exception)
            {
                throw new Exception(ErrorText);
            }
        }

        public static List<Lokomotive> LoadFreeLokomotives()
        {
            return freeLokomotives.ToList();
        }

        public static List<Station> LoadStations()
        {
            //Запрос всех станций в алфавитном порядке
            return context.Stations.OrderBy(s => s.Name).ToList();
        }

        public static List<Code> LoadCodes()
        {
            return context.Codes.ToList();
        }

        public static int AddTrain(Lokomotive lok, Station DepartureStation, Station ArrivalStation)
        {
            Train train = new Train
            {
                //Подтягивание выбранного локомотива
                Lokomotive = lok,
                //Подтягивание списка грузов
                Cargos = cargos,
            };

            //Поиск пути между выбранными станциями
            try
            {
                train.Path = FindPath(DepartureStation, ArrivalStation);
            }
            catch(Exception ex)
            {
                throw ex;
            }
 
            Operation operation = new Operation
            {
                //Присвоение поезду кода операции 9 ("сформирован")
                Code = context.Codes.Where(c => c.CodeId == 9).SingleOrDefault(),
                //Запись текущего времени операции
                Date = DateTime.Now
            };

            train.Weight = ProcessCargoes(train);

            train.Operations.Add(operation);
            train.LastOperation = operation.Code.Name;

            //Присвоение локомотиву поезда кода операции 9 ("сформирован")
            train.Lokomotive.Code = context.Codes.Where(c => c.CodeId == 9).SingleOrDefault();
            //Присвоение локомотиву поезда ссылки на этот поезд
            train.Lokomotive.Train = train;

            // Вставить новые записи в таблицу 
            context.Operations.Add(operation);
            context.Cargos.AddRange(cargos);
            context.Trains.Add(train);

            try
            {
                TrySaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return train.TrainId;
        }

        public static List<int> DelTrains(IList<OperationsView> selectedTrains)
        {
            List<Train> deleteTrains = new List<Train>();
            List<Cargo> linkedCargos = new List<Cargo>();
            List<int> trainIds = new List<int>();
            foreach (OperationsView viewTrain in selectedTrains)
            {
                Train train = viewTrain.train;
                deleteTrains.Add(train);
                trainIds.Add(train.TrainId);
                //Удалить связанные грузы
                linkedCargos = context.Cargos.Where(c => c.Train == train).ToList();
                if (linkedCargos != null)
                    context.Cargos.RemoveRange(linkedCargos);
                //Отвязать локомотивы от поезда
                train.Lokomotive.Train = null;
            }
            context.Trains.RemoveRange(deleteTrains);
            try
            {
                TrySaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return trainIds;
        }

        public static Cargo AddCargo(string name, float weight, decimal cost)
        {
            Cargo cargo = new Cargo(name, weight, cost);
            cargos.Add(cargo);
            return cargo;
        }

        public static void DelCargo(int index)
        {
            try
            {
                cargos.RemoveAt(index);
            }
            catch(ArgumentNullException e)
            {
                throw e;
            }
            return;
        }

        public static void ClearCargos()
        {
            cargos.Clear();
        }

        public static List<OperationsView> LoadActualTrains()
        {
            return actualTrains.ToList(); 
        }

        public static int[] AddOperation(OperationsView selectedView, Code code, DateTime dateTime)
        {
            Train selectedTrain = selectedView.train;
            selectedTrain.Operations.Add(new Operation { Code = code, Date = dateTime });
            selectedTrain.LastOperation = code.Name;
            try
            {
                TrySaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new int[]{code.CodeId, selectedTrain.TrainId};
        }

        public static int[] CancelOperation(OperationsView selectedView)
        {
            Train selectedTrain = selectedView.train;
            List<Operation> operations = context.Trains.Where(t => t.TrainId == selectedTrain.TrainId).Select(t => t.Operations).SingleOrDefault();
            if (operations == null)
            {
                throw new ArgumentNullException("Не найдено ни одной операции с поездом.");
            }
            else if (operations.Count == 1)
            {
                throw new ArgumentException();
            }
            int[] opInfo = { operations.Last().Code.CodeId,selectedTrain.TrainId };
            context.Operations.Remove(operations.Last());
            operations.Remove(operations.Last());
            selectedTrain.LastOperation = operations.Last().Code.Name;
            try
            {
                TrySaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return opInfo;
        }

        public static int UpdateOperation(OperationsView selectedView, DateTime dateTime)
        {
            Train selectedTrain = selectedView.train;
            Operation operation = context.Trains.Where(t => t.TrainId == selectedTrain.TrainId).Select(t => t.Operations).SingleOrDefault().LastOrDefault();
            operation.Date = dateTime;
            try
            {
                TrySaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return selectedTrain.TrainId;
        }
    }
}
