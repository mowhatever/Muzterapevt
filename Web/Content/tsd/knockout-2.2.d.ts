﻿declare module "ko" {
	var _: typeof Ko;
	export = _;
}

declare module Ko {
	export var utils: Ko.Utils;
	export var memoization: Ko.Memoization;
	export var bindingHandlers: Ko.BindingHandlers;
	export var virtualElements: Ko.VirtualElements;
	export var extenders: Ko.Extenders;

	export function applyBindings( viewModel: any, rootNode?: any ): void;
	export function applyBindingsToDescendants( viewModel: any, rootNode: any ): void;
	export function applyBindingsToNode( node: Element, options: any, viewModel: any ): void;

	export var subscribable: Ko.SubscribableStatic;
	export var observable: Ko.ObservableStatic;
	export var computed: Ko.ComputedStatic;
	export var observableArray: Ko.ObservableArrayStatic;

	export function contextFor( node: Element ): any;
	export function contextFor( node: Ko.VirtualElement ): any;
	export function isSubscribable( instance: any ): boolean;
	export function toJSON( viewModel: any, replacer?: Function, space?: any ): string;
	export function toJS( viewModel: any ): any;
	export function isObservable( instance: any ): boolean;
	export function isComputed( instance: any ): boolean;
	export function dataFor( node: any ): any;
	export function removeNode( node: Element );
	export function cleanNode( node: Element );

	export interface SubscribableFunctionsBase<T> {
		dispose(): void;
		peek(): T;
		valueHasMutated(): void;
		valueWillMutate(): void;
	}

	export interface SubscribableFunctions<T> extends SubscribableFunctionsBase<T> {
		extend( source ): Subscribable<T>;
	}

	export interface ComputedFunctions<T> extends SubscribableFunctionsBase<T> {
		extend( source ): Computed<T>;
		getDependenciesCount(): number;
		hasWriteFunction(): boolean;
	}

	export interface ObservableFunctions<T> extends SubscribableFunctionsBase<T> {
		extend( source ): Observable<T>;
	}

	export interface ObservableArrayFunctions<T> extends ObservableFunctions<T[]> {
		// General Array functions
		indexOf( searchElement, fromIndex?: number ): number;
		slice( start: number, end?: number ): T[];
		splice( start: number ): T[];
		splice( start: number, deleteCount: number, ...items: T[] ): T[];
		pop();
		push( ...items: T[] ): void;
		shift();
		unshift( ...items: T[] ): number;
		reverse(): T[];
		sort(): void;
		sort( compareFunction ): void;

		// Ko specific
		replace( oldItem: T, newItem: T ): void;

		remove( item: T ): T[];
		removeAll( items: T[] ): T[];
		removeAll(): T[];

		destroy( item ): void;
		destroyAll( items: T[] ): void;
		destroyAll(): void;
	}

	export interface SubscribableStatic {
		fn: SubscribableFunctions<any>;
		new <T>(): Subscribable<T>;
	}

	export interface Disposable {
		dispose(): void;
	}

	export interface Subscribable<T> extends SubscribableFunctions<T> {
		subscribe( callback: ( newValue: T ) => void , target?: any, topic?: string ): Disposable;
		notifySubscribers( valueToWrite: T, topic?: string );
	}

	export interface ComputedStatic {
		fn: ComputedFunctions<any>;

		(): Computed<any>;
		<T>( func: () => T, context?: any ): Computed<T>;
		<T>( def: ComputedDefine<T> ): Computed<T>;
	}

	export interface Computed<T> extends ComputedFunctions<T> {
		(): T;
		( value: T ): void;

		subscribe( callback: ( newValue: T ) => void , target?: any, topic?: string ): Disposable;
		notifySubscribers( valueToWrite: T, topic?: string );
		extend( extenders: any ): Computed<T>;
	}

	export interface ObservableArrayStatic {
		fn: ObservableArrayFunctions<any>;
		<T>(): ObservableArray<T>;
	}

	export interface ObservableArray<T> extends ObservableArrayFunctions<T> {
		(): T[];
		( value: T[] ): void;

		subscribe( callback: ( newValue: T[] ) => void , target?: any, topic?: string ): Disposable;
		notifySubscribers( valueToWrite: T[], topic?: string );
	}

	export interface ObservableStatic {
		fn: ObservableFunctions<any>;
		<T>( value?: T ): Observable<T>;
	}

	export interface Observable<T> extends ObservableFunctions<T> {
		(): T;
		( value: T ): any;

		subscribe( callback: ( newValue: T ) => void , target?: any, topic?: string ): Disposable;
		notifySubscribers( valueToWrite: T, topic?: string );
	}

	export interface ComputedDefine<T> {
		read: { (): T; };
		write?: { ( value: T ): void; };
		owner?: any;
		deferEvaluation?: boolean;
		disposeWhen?: () => boolean;
		disposeWhenNodeIsRemoved?: Element;
	}

	export interface BindingContext {
		$parent: any;
		$parents: any[];
		$root: any;
		$data: any;
		$index?: number;
		$parentContext?: BindingContext;

		extend( any ): any;
		createChildContext( any ): any;
	}

	export interface BindingHandlerResult {
		controlsDescendantBindings?: boolean;
	}

	export interface BindingHandlerBase {
		update? ( element: any, valueAccessor: () => any, allBindingsAccessor: () => any, viewModel: any, bindingContext: BindingContext ): void;
		options?: any;
	}

	export interface BindingHandler extends BindingHandlerBase {
		init? ( element: any, valueAccessor: () => any, allBindingsAccessor: () => any, viewModel: any, bindingContext: BindingContext ): BindingHandlerResult;
	}

	export interface BindingHandlerWithNoResult extends BindingHandlerBase {
		init? ( element: any, valueAccessor: () => any, allBindingsAccessor: () => any, viewModel: any, bindingContext: BindingContext ): void;
	}

	export interface BindingHandlers {
		// Controlling text and appearance
		visible: BindingHandler;
		text: BindingHandler;
		html: BindingHandler;
		css: BindingHandler;
		style: BindingHandler;
		attr: BindingHandler;

		// Control Flow
		foreach: BindingHandler;
		if: BindingHandler;
		ifnot: BindingHandler;
		with: BindingHandler;

		// Working with form fields
		click: BindingHandler;
		event: BindingHandler;
		submit: BindingHandler;
		enable: BindingHandler;
		disable: BindingHandler;
		value: BindingHandler;
		hasfocus: BindingHandler;
		checked: BindingHandler;
		options: BindingHandler;
		selectedOptions: BindingHandler;
		uniqueName: BindingHandler;

		// Rendering templates
		template: BindingHandler;

		[key: string]: BindingHandler;
		[key: string]: BindingHandlerWithNoResult;
	}

	export interface Memoization {
		memoize( callback );
		unmemoize( memoId, callbackParams );
		unmemoizeDomNodeAndDescendants( domNode, extraCallbackParamsArray );
		parseMemoText( memoText );
	}

	export interface VirtualElement { }

	export interface VirtualElements {
		allowedBindings: { [bindingName: string]: boolean; };
		emptyNode( e: VirtualElement );
		firstChild( e: VirtualElement );
		insertAfter( container: VirtualElement, nodeToInsert: HTMLElement, insertAfter: HTMLElement );
		nextSibling( e: VirtualElement );
		prepend( e: VirtualElement, toInsert: HTMLElement );
		setDomNodeChildren( e: VirtualElement, newChildren: { length: number;[index: number]: HTMLElement; });
		childNodes( e: VirtualElement ): HTMLElement[];
	}

	export interface Extenders {
		throttle<T>( target: Observable<T>, timeout: number ): Computed<T>;
		notify<T>( target: Observable<T>, notifyWhen: string ): Computed<T>;
	}

	export interface Utils {

		fieldsIncludedWithJsonPost: any[];

		arrayForEach<T>( array: T[], action: ( i: T ) => void ): void;
		arrayIndexOf<T>( array: T[], item: T ): number;
		arrayFirst<T>( array: T[], predicate: ( item: T ) => boolean, predicateOwner?: any ): any;
		arrayRemoveItem<T>( array: T[], itemToRemove: T ): void;
		arrayGetDistinctValues<T>( array: T[] ): T[];
		arrayMap<T, U>( array: T[], mapping: ( item: T ) => U ): U[];
		arrayFilter<T>( array: T[], predicate: ( item: T ) => boolean ): T[];
		arrayPushAll<T>( array: T[], valuesToPush: T[] ): T[];

		extend( target, source );

		emptyDomNode( domNode ): void;
		moveCleanedNodesToContainerElement( nodes: any[] ): HTMLElement;
		cloneNodes( nodesArray: any[], shouldCleanNodes: boolean ): any[];
		setDomNodeChildren( domNode: any, childNodes: any[] ): void;
		replaceDomNodes( nodeToReplaceOrNodeArray: any, newNodesArray: any[] ): void;
		setOptionNodeSelectionState( optionNode: any, isSelected: boolean ): void;
		stringTrim( str: string ): string;
		stringTokenize( str: string, delimiter: string ): string;
		stringStartsWith( str: string, startsWith: string ): string;
		domNodeIsContainedBy( node: any, containedByNode: any ): boolean;
		domNodeIsAttachedToDocument( node: any ): boolean;
		tagNameLower( element: any ): string;
		registerEventHandler( element: any, eventType: any, handler: Function ): void;
		triggerEvent( element: any, eventType: any ): void;
		
		unwrapObservable( value: any ): any;
		unwrapObservable<T>( value: T ): T;
		unwrapObservable<T>( value: Observable<T> ): T;

		peekObservable( value: any ): any;
		peekObservable<T>( value: T ): T;
		peekObservable<T>( value: Observable<T> ): T;

		makeArray( arrayLikeObject: any ): any[];
		makeArray<T>( arrayLikeObject: { length: number; [idx: number]: T; } ): T[];

		toggleDomNodeCssClass( node: any, className: string, shouldHaveClass: boolean ): void;
		setTextContent( element: any, textContent: string ): void;
		setElementName( element: any, name: string ): void;
		forceRefresh( node: any ): void;
		ensureSelectElementIsRenderedCorrectly( selectElement: any ): void;
		range( min: number, max: number ): number[];
		range( min: Observable<number>, max: Observable<number> ): number[];
		getFormFields( form: any, fieldName: string ): any[];
		parseJson( jsonString: string ): any;
		stringifyJson( data: any, replacer: Function, space: string ): string;
		postJson( urlOrForm: any, data: any, options: any ): void;

		ieVersion: number;
		isIe6: boolean;
		isIe7: boolean;

		domNodeDisposal;
	}
}