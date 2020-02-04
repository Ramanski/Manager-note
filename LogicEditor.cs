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
        private SampleContext context;
        private List<Cargo> cargoes = new List<Cargo>();
        public int powerKind { get; set; }
        public int trainType { get; set; }

        public LogicEditor()
        {
            try
            {
            // !!Построение БД заново, если модель изменилась!!
//          Database.SetInitializer(new DropCreateDatabaseAlways<SampleContext>());
            context = new SampleContext();
            }
            catch(Exception)
            {
                throw new Exception();
            }
            //Логгирование запросов к БД
            context.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
        }

        private Path FindPath(Station DepartureStation, Station ArrivalStation)
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

        private float ProcessCargoes(Train train)
        {
            float Weight = 0;
            foreach (Cargo cargo in cargoes)
            {
                cargo.Train = train;
                cargo.CostToTransport *= train.Path.Distance;
                Weight += cargo.Weight;
            }
            return Weight;
        }

        // Сохранить изменения в БД
        public void TrySaveChanges(SampleContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                throw new Exception("Произошла ошибка при обновлении записей.");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException)
            {
                throw new Exception("Произошла ошибка при валидации полученных данных.");
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Произошла ошибка записи в базу данных.");
            }
            catch (Exception)
            {
                throw new Exception("Произошла непредвиденная ситуация");
            }
        }

        public List<Lokomotive> LoadFreeLokomotives()
        {
            //Запрос свободных локомотивов со статусом "брошен" или "прибыл"
            var freeLokomotives = context.Lokomotives
                                     .Where(l => (l.Train == null) &&
                                            l.PowerKind.PowerKindId == powerKind &&
                                            l.TrainType.TrainTypeId == trainType);
            return freeLokomotives.ToList();
        }

        public List<Station> LoadStations()
        {
            //Запрос всех станций в алфавитном порядке
            return context.Stations.OrderBy(s => s.Name).ToList();
        }

        public List<Code> LoadCodes()
        {
            return context.Codes.Where(c=>c.CodeId >= 200).ToList();
        }

        public int AddTrain(Lokomotive lok, Station DepartureStation, Station ArrivalStation)
        {
            Train train = new Train
            {
                //Подтягивание выбранного локомотива
                Lokomotive = lok,
                //Подтягивание списка грузов
                Cargoes = cargoes,
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
                Code = context.Codes.Where(c => c.CodeId == 9).Single(),
                //Запись текущего времени операции
                Date = DateTime.Now
            };

            train.Weight = ProcessCargoes(train);
            train.Operations.Add(operation);
            train.LastOperation = operation.Code;

            //Присвоение локомотиву поезда кода операции 9 ("сформирован")
            train.Lokomotive.Code = context.Codes.Where(c => c.CodeId == 9).SingleOrDefault();
            //Присвоение локомотиву поезда ссылки на этот поезд
            train.Lokomotive.Train = train;

            // Вставить новые записи в таблицу 
            context.Operations.Add(operation);
            context.Cargoes.AddRange(cargoes);
            context.Trains.Add(train);

            try
            {
                TrySaveChanges(context);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return train.TrainId;
        }

        public Cargo AddCargo(string name, float weight, decimal cost)
        {
            Cargo cargo = new Cargo(name, weight, cost);
            cargoes.Add(cargo);
            return cargo;
        }

        public void DelCargo(int index)
        {
            try
            {
                cargoes.RemoveAt(index);
            }
            catch(ArgumentNullException e)
            {
                throw e;
            }
            return;
        }

        public void ClearCargos()
        {
            cargoes.Clear();
        }

        public List<OperationsView> LoadActualTrains()
        {
            //Выборка сведений по всем поездам
           var actualTrains = context.Trains
                                    .Select(t => new
                                    {
                                        num = t.TrainId,
                                        stot = t.Path.DepartureStation.Name,
                                        stnz = t.Path.ArriveStation.Name,
                                        oper = t.LastOperation.Name,
                                        time = t.Operations.OrderByDescending(o => o.OperationId).FirstOrDefault().Date,
                                    })
                                    .AsEnumerable()
                                    .Select(an => new OperationsView
                                    {
                                        stot = an.stot,
                                        stnz = an.stnz,
                                        oper = an.oper,
                                        time = an.time,
                                        trainId = an.num
                                    });
            return actualTrains.ToList(); 
        }

        private void checkCodeSequence(int prevCode, int curCode)
        {
            if (prevCode == curCode)
                    throw new Exception($"Данная операция ({prevCode}) уже была произведена");
            if (curCode == 204 || prevCode == 204)
                return;
            if ((prevCode == 9 && curCode == 200) || (prevCode == 200 && curCode == 201) || (prevCode == 201 && curCode == 205))
                return;
            throw new Exception($"Нарушение логической последовательности операций: {prevCode} -> {curCode}") ;
        }

        public int[] AddOperation(OperationsView selectedView, Code code, DateTime dateTime)
        {
            Train selectedTrain = context.Trains.Where(t => t.TrainId == selectedView.trainId)
                                                .Include(t=>t.LastOperation)
                                                .Include(t=>t.Lokomotive)
                                                .SingleOrDefault();
            checkCodeSequence(selectedTrain.LastOperation.CodeId, code.CodeId);
            selectedTrain.Operations.Add(new Operation { Code = code, Date = dateTime });
            selectedTrain.LastOperation = code;
            if (code.CodeId == 205)
            {
                selectedTrain.Lokomotive.Train = null;
                selectedTrain.Lokomotive.Code = context.Codes.Where(c => c.CodeId == 1).SingleOrDefault();
            }
            selectedTrain.Lokomotive.Code = code;
            TrySaveChanges(context);
            return new int[]{code.CodeId, selectedTrain.TrainId};
        }

        public int DelTrain(OperationsView selectedView)
        {
            Train selectedTrain = context.Trains
                                         .Where(t => t.TrainId == selectedView.trainId)
                                         .Include(t=>t.Lokomotive)
                                         .SingleOrDefault();
            //Отвязать локомотив от поезда
            selectedTrain.Lokomotive.Train = null;
            selectedTrain.Lokomotive.Code = context.Codes.Where(c => c.CodeId == 204).First();
            context.Trains.Remove(selectedTrain);
            TrySaveChanges(context);
            return selectedTrain.TrainId;
        }
        
        public int[] CancelOperation(OperationsView selectedView)
        {
            Train selectedTrain = context.Trains
                                         .Where(t => t.TrainId == selectedView.trainId)
                                         .Include(t => t.Lokomotive)
                                         .SingleOrDefault();
            List<Operation> operations = getOperations(selectedView);
            if (operations == null)
            {
                throw new ArgumentNullException("Не найдено ни одной операции с поездом.");
            }
            else if (operations.Count == 1)
            {
                throw new ArgumentException();
            }
            int[] opInfo = { operations.Last().Code.CodeId, selectedTrain.TrainId };
            Operation deleteOperation = operations.Last();
            context.Operations.Remove(deleteOperation);
            operations.Remove(deleteOperation);
            selectedTrain.Lokomotive.Code = operations.Last().Code;
            selectedTrain.LastOperation = operations.Last().Code;
            TrySaveChanges(context);
            return opInfo;
        }

        public int UpdateOperation(OperationsView selectedView, DateTime dateTime)
        {
            Train selectedTrain = context.Trains
                                         .Where(t => t.TrainId == selectedView.trainId)
                                         .Include(t=>t.Operations)
                                         .SingleOrDefault();
            selectedTrain.Operations.Last().Date = dateTime;
            TrySaveChanges(context);
            return selectedTrain.TrainId;
        }

        public List<Operation> getOperations(OperationsView selectedView)
        {
            return context.Trains.Where(t => t.TrainId == selectedView.trainId).Select(t => t.Operations).Single();
        }

        public int getTrainId(OperationsView selectedView)
        {
            return selectedView.trainId;
        }
    }
}
