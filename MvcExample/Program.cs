// See https://aka.ms/new-console-template for more information
using MvcExample.Puzzle15;

Console.WriteLine("Hello, World!");

var view = new View();
var model = Model.CreateRandom();
var controller = new Controller();

MvcLoop.Run(model, view, controller);






