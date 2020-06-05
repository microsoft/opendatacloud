// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Directive, Input, TemplateRef, ViewContainerRef} from '@angular/core';

export class AppVarContext {
    $implicit: any = null;
    appVar: any = null;
}

@Directive({
    selector: '[appVar]'
})
export class AppVarDirective {
    private _context = new AppVarContext();

    @Input()
    set appVar(value: any) {
        this._context.$implicit = this._context.appVar = value;
    }

    constructor(_vcr: ViewContainerRef, _templateRef: TemplateRef<AppVarContext>) {
        _vcr.createEmbeddedView(_templateRef, this._context);
    }
}
