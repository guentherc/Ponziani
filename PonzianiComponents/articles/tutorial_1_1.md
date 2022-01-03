# Tutorial - Preparations

First we need to have a running Blazor application. To do this create a Blazor application - both Blazor Server or Blazor WASM 
are ok. For this tutorial we will use Blazor WASM. You can do this either via Visual Studio or via the dotnet command line tools:

`dotnet new blazorwasm -f net6.0 --hosted --name PonzianiTutorial`

If you use Visual Studio to create the app, please make sure that you check the 'ASP.Net Core hosted' checkbox. We need this as the 
embedded Stockfish engine requires special security settings.

Open the Program.cs file in the PonzianiTutorial.Server Project and add the bold lines 

<pre>
    ...

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    <strong>
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
        context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
        context.Response.Headers.Add("Cross-Origin-Resource-Policy", "same-site");
        await next();
    });
    </strong>
    app.UseHttpsRedirection();

    ...
</pre>

Last preparation step is the installation of the PonzianiComponents Package:

`dotnet add package PonzianiComponents --version 0.5.0`

and to add the necessary imports to File _Imports.razor

    @using PonzianiComponents
    @using PonzianiComponents.Chesslib
    @using PonzianiComponents.Chesslib.UCIEngine

Now we are done and you can start your new application tocheck if it's running:
Switch to the Directory /PonzianiTutorial/Server and enter

`dotnet run --urls=https://localhost:5001/`

on the command line and then direct your browser to https://localhost:5001/ and you should see the blazor template app in action.

> [!div class="nextstepaction"]
> [Next: Chessboard with automatic engine analysis](tutorial_1_2.md)

