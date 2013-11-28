﻿/// <amd-dependency path="text!./Templates/Attachment.html" />
/// <amd-dependency path="css!./Templates/Attachment.css" />
import c = require( "../common" );
import $ = require( "jQuery" );
import rx = require( "rx" );
import ko = require( "ko" );

export interface AttachmentDef {
	Path: string;
	Download: string;
	IconClass?: string;
	SmallThumb?: string;
	BigThumb?: string;
}

export interface IAttachment {
	Render(): c.IControl;
	AsBBCode(): string;
}

export class File implements IAttachment {
	constructor( public Def: AttachmentDef ) { }

	Render(): c.IControl {
		return new Visual( this.Def, Templates.File );
	}
	AsBBCode() {
		return "[file=" + this.Def.Path + "]" + this.Def.Path + "[/file]";
	}
}

export class Picture implements IAttachment {
	constructor( public Def: AttachmentDef ) { }

	Render(): c.IControl {
		return new Visual( this.Def, Templates.Picture );
	}
	AsBBCode() {
		return "[img=" + this.Def.Path + "]";
	}
}

class Visual implements c.IControl {
	constructor( public Def: AttachmentDef, template: ( e: Element ) => void ) {
		this.OnLoaded = template;
	}

	OnLoaded: ( e: Element ) => void;
	ControlsDescendantBindings = true;
}

module Templates {
	var t = $( require( "text!./Templates/Attachment.html" ) );
	export var File = c.ApplyTemplate( t.filter( ".attachment-file" ) );
	export var Picture = c.ApplyTemplate( t.filter( ".attachment-picture" ) );
}