$MENU::MaxItems = 8;
$MENU::Text[ "Back" ] = "...Back";
$MENU::Text[ "Next" ] = "Next...";
$MENU::Delay = ( 1 / 60 );

function menu::new( %displayName, %menuHandle, %cl, %locked )
{
	%cl.menuTitle = %displayName;
	%cl.menuMode = %menuHandle;
	%cl.menuLock = %locked;
	%cl.menuKey++;
	%cl.menuItems = -1;
  
  schedule( sprintf( "menu::show( %1, 0 );", $MENU::Delay, %cl );
}

function menu::add( %item, %code, %cl, %condition )
{
	if( %condition || ( %condition == "" ) )
	{
		%cl.menuItems[ %cl.menuItems++ ] = %item;
		%cl.menuItems[ %cl.menuItems, Code ] = %code;
		%cl.menuKey[ %code ] = %cl.menuKey;
	}
}

function menu::show( %cl, %startPos )
{
   Client::setMenuScoreVis( %cl, true );
   remoteEval( %cl, "NewMenu", %cl.menuTitle );

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
      remoteEval( %cl, "AddMenuItem", %curItem++ @ $MENU::Text[ "Back" ], "%menuPos " @ %backPos );
   }

   for( %curPos; ( %curPos <= %cl.menuItems ); %curPos++ )
   {
      if( ( %curPos == %endPos ) && ( %curPos < %cl.menuItems ) )
      {
         remoteEval( %cl, "AddMenuItem", %curItem++ @ $MENU::Text[ "Next" ], "%menuPos " @ %curPos );
         break;
      }
      remoteEval( %cl, "AddMenuItem", %curItem++ @ %cl.menuItems[ %curPos ], %cl.menuItems[ %curPos, Code ] );
   }
}

function Game::menuRequest( %cl )
{
	if( %cl.selClient && ( %cl.selClient != %cl ) )
		menu::nonself( %cl );
	else if ( %cl.selClient == %cl )
		menu::self( %cl );
	else if( $vote::Topic != "" && ( %cl.vote == "" || %cl.canCancelVote ) )
		menu::votepending( %cl );
	else if( %cl.adminLevel )
		menu::admin( %cl );
	else 
		menu::Vote( %cl );
}

function remoteMenuSelect( %cl, %code )
{
	// no quotes or escapes
	if( String::findSubStr( %code, "\"" ) != -1 || String::findSubStr( %code, "\\" ) != -1 )
		return;
	
	// scroll
	if( !String::ICompare( getWord( %code, 0 ), "%menuPos" ) )
		return Client::showMenu( %cl, getWord( %code, 1 ) );
   
	// validate menu option
	if( String::ICompare( %cl.menuKey[ %code ], %cl.menuKey ) )
		return;

	%eval = "processMenu" @ %cl.menuMode @ "(" @ %cl @ ", \"" @ %code @ "\");";

	%cl.menuMode = "";
	%cl.menuLock = "";
	dbecho( 2, "MENU: " @ %cl @ "- " @ %eval );
	eval( %eval );
	
	if( %cl.menuMode == "" )
	{
		Client::cancelMenu( %cl );
	}
}

function Client::cancelMenu( %cl )
{
	if( %cl.menuLock )
		return;

	%cl.selClient = "";
	%cl.menuMode = "";
	%cl.menuLock = "";
	remoteEval( %cl, "CancelMenu" );
	Client::setMenuScoreVis( %cl, false );
}

function Client::buildMenu( %cl, %menuTitle, %menuCode, %cancellable )
{
	Client::setMenuScoreVis( %cl, true );
	%cl.menuLock = !%cancellable;
	%cl.menuMode = %menuCode;
	remoteEval( %cl, "NewMenu", %menuTitle );
}

function Client::addMenuItem( %cl, %option, %code )
{
	remoteEval( %cl, "AddMenuItem", %option, %code );
}
