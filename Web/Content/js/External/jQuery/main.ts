﻿/// <reference path="../../../tsd/require.d.ts" />

require.config( {
	map: {
		'app/External/jQuery/jquery-ui-1.10.3.custom.min': {
			jQuery: 'app/External/Rx/rx.min'
		}
	}
});

define( [
	'app/External/jQuery/jQuery-1.9.1',
	'app/External/jQuery/jQuery-ui-1.10.3.custom.min',
	'css!styles/jQuery/jQuery-ui-1.10.3.custom.min.css'],
	$ => $ );