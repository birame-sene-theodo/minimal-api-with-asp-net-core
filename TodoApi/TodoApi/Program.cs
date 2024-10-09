using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/",GetAllTodos);

todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}",
   GetTodo);


todoItems.MapPost("/", CreateTodo);

todoItems.MapPut("/{id}", UpdateTodo);

todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(d => new TodoItemDTO(d)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id) is Todo todo ? TypedResults.Ok(new TodoItemDTO(todo)) : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todo, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todo.IsComplete,
        Name = todo.Name
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    var dto = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{dto.Id}");
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}