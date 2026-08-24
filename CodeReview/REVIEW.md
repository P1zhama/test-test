
НАЙДЕННЫЕ ПРОБЛЕМЫ TimesheetReportHandle.cs

 Две проблемы, которые исправил прямо в коде
1. Неверный выбор СТАВКИ сотрудника.
2. double у денег вместо decimal

3. Полная выгрузка коллекции в память перед фильтрацией. Фильтровать на стороне Mongo 

// periodStart и periodEnd для использования индекса
var periodStart = new DateTime(request.Year, request.Month, 1);
var periodEnd = periodStart.AddMonths(1);

.Find(e => e.Date >= periodStart && e.Date < periodEnd)

4. Запросы N + 1 к employees и projects внутри foreach.
   Как вариант - собрать все уникальные EmployeeId и ProjectId из monthEntries и подгрузить их двумя запросами в бд,
   cложить их в словари, дальше обращаться к словарям в цикле.
   .Find(Builders<Employee>.Filter.In(e => e.Id, ids))
   .Find(Builders<Project>.Filter.In(p => p.Id, ids))

5. .Result вместо await. Получается синхронное ожидание, блокируем поток из пула потоков.
6. CancellationToken никуда не пробрасывается
7. Отсутствуют проверки на null. NullReferenceException. Если employee или project не найдены.
8. Возможно деление на ноль Math.Round(row.Amount / row.Budget * 100, 2);

 Что бы изменил в структуре
1. По коду видно, что используется CQRS + MediatR. Нет валидатора для проверки данных в GetProjectReportQuery.
2. Нет логирования. Можно использовать Serilog
3. Как вариант - применить паттерн репозиторий, чтобы не работать с IMongoDatabase напрямую


НАЙДЕННЫЕ ПРОБЛЕМЫ TimeEntriesPage.tsx

   Первые две проблемы исправил в файле TimeEntriesPage.fixed.tsx        
1. useEffect(() => { load(); }) без массива зависимостей - бесконечный цикл запросов,
   так как load() вызывает setEntries, что триггерит новый рендер.
   Исправить можно так: useEffect(() => { load(); }, [props.year, props.month])

2. entries.push(body); setEntries(entries) - в entries пишется тело запроса, а не ответа от сервера
   После УСПЕШНОГО сохранения вызвать await load()

3. Отсутствие обработки ошибок сервера. Необходимо проверять response.ok 
   и при ошибке - показывать пользователю текст этой ошибки, а не просто выдавать alert("Сохранено")

4. Нет пагинации в load(). 

5. Отсутствует валидация данных. Например: в hours может быть пустая страка при отправке на сервер

6. useState<any[]>([]). any[] вместо типов. Решение - создать отдельные интерфейсы. 
   Например: TimeEntryDto, EmployeeDto

7. В entries.filter((e) => e.employeeId == employeeId) используется не строгое равенство == вместо строгого ===

8. Нет подтверждения удаления. Сразу вызывается remove(entry.id). Добавить окно подтверждения удаления


 Что бы изменил в структуре
1. Вынести fetch - запросы в отдельный слой (хук useTimeEntries() useEmployees())
2. Централизировать обработку ошибок, чтобы не городить разрные реализации