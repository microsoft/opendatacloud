import { NgModule } from '@angular/core';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';

@NgModule({
    imports: [
        PaginationModule.forRoot(),
        TypeaheadModule.forRoot(),
        BsDatepickerModule.forRoot()
    ],
    exports: [
        PaginationModule,
        TypeaheadModule,
        BsDatepickerModule
    ]
})

export class UIComponentsModule {
}
