$MENU::MaxItems = 8;
$MENU::Text[ "Back" ] = "...Back";
$MENU::Text[ "Next" ] = "Next...";
$MENU::Delay = ( 1 / 60 );

function Client::cancelMenu( %clientId )
{
   if( !%clientId.menuLock )
   {
      %clientId.selClient = "";
      %clientId.menuMode = "";
      %clientId.menuLock = "";
      remoteEval( %clientId, "CancelMenu" );
      Client::setMenuScoreVis( %clientId, false );
   }
}

function Client::buildMenu( %clientId, %menuTitle, %menuCode, %locked )
{
   %clientId.menuLock = %locked;
   %clientId.menuMode = %menuCode;
   %clientId.menuTitle = %menuTitle;

   %clientId.menuKey++;
   %clientId.menuItems = -1;

   schedule( sprintf( "Client::showMenu(%1, 0);", %clientId ), $MENU::Delay, %clientId );
}

function Client::addMenuItem( %clientId, %option, %code, %condition )
{
   if( String::Empty( %condition ) || %condition )
   {
      %clientId.menuItem[ %clientId.menuItems++ ] = %option;
      %clientId.menuCode[ %clientId.menuItems ] = %code;
      %clientId.menuKey[ %code ] = %clientId.menuKey;
   }
}

function Client::showMenu( %clientId, %startPos )
{
   Client::setMenuScoreVis( %clientId, true );
   remoteEval( %clientId, "NewMenu", %clientId.menuTitle );

   %curItem = 0;
   %curPos = %startPos;
   %endPos = ( %curPos + $MENU::MaxItems - 1 );
   
   if( %curPos > 0 )
   {
      %endPos--;
      if( %curPos != 7 )
         %backPos = ( %curPos - 6 );
      else
         %backPos = ( %curPos - 7 );
      
      Client::showMenuItem( %clientId, %curItem++ @ $MENU::Text[ "Back" ], "%menuPos " @ %backPos );
   }

   for( %curPos; ( %curPos <= %clientId.menuItems ); %curPos++ )
   {
      if( ( %curPos == %endPos ) && ( %curPos < %clientId.menuItems ) )
      {
         Client::showMenuItem( %clientId, %curItem++ @ $MENU::Text[ "Next" ], "%menuPos " @ %curPos );
         break;
      }
      Client::showMenuItem( %clientId, %curItem++ @ %clientId.menuItem[ %curPos ], %clientId.menuCode[ %curPos ] );
   }
}

function Client::showMenuItem( %clientId, %option, %code )
{
   remoteEval( %clientId, "AddMenuItem", %option, %code );
}

function remoteMenuSelect( %clientId, %code )
{
   %mm = %clientId.menuMode;
   if( %mm == "" )
      return;

   // client is trying to be bad!
   if( String::findSubStr( %code, "\"" ) != -1 || String::findSubStr( %code, "\\" ) != -1 ) // no quotes or escapes
      return;

   // client is scrolling through the menu options
   if( getWord( %code, 0 ) == "%menuPos" )
      return Client::showMenu( %clientId, getWord( %code, 1 ) );

   // client submitted an option not available to them
   if( %clientId.menuKey[ %code ] != %clientId.menuKey )
      return;

   // if "#" precedes the menu mode, then we will go to the plain-titled function, but will go to "processMenu" function if it is not present
   if( String::getSubStr( %code, 0, 1 ) != "#" )
      %mm = sprintf( "processMenu%1", %mm );

   %evalString = sprintf( "%1( %2, \"%3\" );", %mm, %clientId, %code );

   %clientId.menuMode = "";
   %clientId.menuLock = "";

   dbecho( 2, "MENU: " @ %clientId @ "- " @ %evalString );

   eval( %evalString );

   // if client no longer has a menu mode, we call function to cancel menu out for client
   if( %clientId.menuMode == "" )
      Client::cancelMenu( %clientId );
}


// Client Functions

function remoteCancelMenu( %server )
{
   if( %server != 2048 )
      return;
   if( isObject( CurServerMenu ) )
      deleteObject( CurServerMenu );
}

function remoteNewMenu( %server, %title )
{
   if( %server != 2048 )
      return;

   if( isObject( CurServerMenu ) )
      deleteObject( CurServerMenu );

   newObject( CurServerMenu, ChatMenu, %title );
   setCMMode( PlayChatMenu, 0 );
   setCMMode( CurServerMenu, 1 );
}

function remoteAddMenuItem( %server, %title, %code )
{
   if( %server != 2048 )
      return;
   addCMCommand( CurServerMenu, %title, clientMenuSelect, %code );
}

function clientMenuSelect( %code )
{
   deleteObject( CurServerMenu );
   remoteEval( 2048, menuSelect, %code );
}
