﻿/// <amd-dependency path="css!styles/common.css" />
/// <amd-dependency path="css!styles/bl.css" />
import c = require( "./common" );
import $ = require( "jQuery" );
import ab = require( "./Lib/autobind" );

ab.autobindAll( $( ".autobind" ) );