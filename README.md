# RaspCommander
Just a simple file manager.
![Program image](/Screenshots/Main.png)

## Functionality
### Drag & Drop
Drag a file or folder from your desktop to any of the grids to copy it.
You can do the same between the grids.
### Context menu
Right-click on any file or folder to delete it, create a new folder or to refresh the view.
![Context menu](/Screenshots/Menu.png)
### Exploring
Double-click on any file or folder to enter it. Do this for the ".." folder to move up.
### Sorting
Click a header to sort the grid by it.
![Sorting example](/Screenshots/Sort.png)

## Asynchronous actions
Whenever you delete or copy any item, the UI will become disabled till the completion of the operation,
thus allowing the program not to become detected as "not responding" by Windows.
![UI during operation](/Screenshots/Async.png)
