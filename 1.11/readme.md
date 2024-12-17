# Tribes 1.11 Menu Functions

## Scrolling Menus

To implement these functions, you need to make a slight change to how menu items are defined in functions that generate and populate menus in the game.
- Remove all numerical prefixes from menu items (eg. `%curItem++`, etc.).
- Remove any redundant loops for menus that have more than 8 options (eg. mission selection menus).
- Remove the "true" argument at the end of calls to `Client::buildMenu()`. This was `%cancellable`, but is now `%locked` in the new menu.cs functions.

Here is an example of the `Admin::changeMissionMenu()` function from `admin.cs`:
``` c#
function Admin::changeMissionMenu( %clientId )
{
  Client::buildMenu( %clientId, "Pick Mission Type", "cmtype" );
  %index = 1;
  //DEMOBUILD - the demo build only has one "type" of missions
  if( $MList::TypeCount < 2 ) $TypeStart = 0;
  else $TypeStart = 1;
  for( %type = $TypeStart; %type < $MLIST::TypeCount; %type++ )
    if( $MLIST::Type[ %type ] != "Training" )
      Client::addMenuItem( %clientId, $MLIST::Type[ %type ], %type @ " 0") ;
}
```
This will generate a paging menu when there are more than 8 mission types available on the server.

Here is an example of an updated `processMenuCMType()` that no longer includes options for "more" mission types and instead just displays the missions for that type (without the "more" missions option):
``` c#
function processMenuCMType( %clientId, %options )
{
  %option = getWord( %options, 0 );
  %first = getWord( %options, 1 );
  Client::buildMenu( %clientId, "Pick Mission", "cmission" );

  for( %i = 0; ( %misIndex = getWord( $MLIST::MissionList[ %option ], %first + %i ) ) != -1; %i++ )
    Client::addMenuItem( %clientId, $MLIST::EName[ %misIndex ], %misIndex @ " " @ %option );
}
```

Lastly, here is `processMenuCMission()`, that concludes the process and executes a mission change. Again, the "more" check that would have looped back to `processMenuCMtype()` has been removed:
``` c#
function processMenuCMission( %clientId, %option )
{
   %mi = getWord( %option, 0 );
   %mt = getWord( %option, 1 );

   %misName = $MLIST::EName[ %mi ];
   %misType = $MLIST::Type[ %mt ];

   // verify that this is a valid mission:
   if( %misType == "" || %misType == "Training" )
      return;
   for( %i = 0; true; %i++ )
   {
      %misIndex = getWord( $MLIST::MissionList[ %mt ], %i );
      if( %misIndex == %mi )
         break;
      if( %misIndex == -1 )
         return;
   }
   if( %clientId.isAdmin )
   {
      messageAll( 0, Client::getName( %clientId ) @ " changed the mission to " @ %misName @ " (" @ %misType @ ")" );
      Vote::changeMission();
      Server::loadMission( %misName );
   }
   else
   {
      Admin::startVote( %clientId, "change the mission to " @ %misName @ " (" @ %misType @ ")", "cmission", %misName );
      Game::menuRequest( %clientId );
   }
}
```
