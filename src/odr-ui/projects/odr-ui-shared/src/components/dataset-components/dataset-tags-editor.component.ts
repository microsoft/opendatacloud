import { Component, Input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable, ReplaySubject } from 'rxjs';
import { TagsService } from '../../services/tags.service';
import { addToSortedList, removeFromSortedList } from '../../utils/list-utils';
import { TypeaheadMatch } from 'ngx-bootstrap';
import { tap, map } from 'rxjs/operators';
import { isEqual } from 'lodash';

export const datasetTagsFormControl = () => new FormControl();

@Component({
    selector: 'app-dataset-tags-editor',
    template: `
        <div class="form-group col-lg-6" [ngClass]="{ 'has-error' : hasErrors() }">
            <label class="control-label">
                Dataset Tags
                <input [(ngModel)]="tagTextEntry"
                       [ngModelOptions]="{standalone: true}"
                       [typeahead]="tagsDataSource"
                       (typeaheadLoading)="changeTypeaheadLoading($event)"
                       (typeaheadNoResults)="changeTypeaheadNoResults($event)"
                       (typeaheadOnSelect)="typeaheadOnSelect($event)"
                       [typeaheadOptionsLimit]="7"
                       (keydown)="addNewTag($event)"
                       typeaheadOptionField="name"
                       placeholder="Add dataset tags ..."
                       class="form-control"
                       [disabled]="isDisabled">
            </label>
            <div class="help-block" *ngIf="controlErrors('tags')">
                {{controlErrors('tags').message}}
            </div>
            <div class="tag-lookup" *ngIf="typeaheadLoading || typeaheadNoResults">
                <div *ngIf="typeaheadLoading">Loading ...</div>
                <div *ngIf="typeaheadNoResults">&#10060; No Results Found</div>
            </div>
            <div>
                <div *ngFor="let tag of control.value" class="tag">
                    <button type="button" class="btn btn-default btn-sm" (click)="removeTag(tag)"
                        [disabled]="isDisabled" *ngIf="tag">
                        {{tag}}
                        <span class="badge">&times;</span>
                    </button>
                </div>
            </div>
        </div>
    `,
    styleUrls: ['./dataset-tags-editor.component.scss']
})
export class DatasetTagsEditorComponent implements OnInit {

    @Input() control: FormControl;

    tags: Observable<any>;
    tagTextEntry: string;
    tagsDataSource: Observable<any>;
    typeaheadLoading = false;
    typeaheadNoResults = false;
    originalValue: string[];

    constructor(
        private tagsService: TagsService,
    ) {
    }

    ngOnInit(): void {
        this.originalValue = this.control.value;
        const tagsList = new ReplaySubject(1);
        this.tagsService.getTagsList()
            .pipe(
                tap((tags) => {
                    tagsList.next(tags);
                })
            ).subscribe();

        this.tagsDataSource = tagsList.asObservable()
            .pipe(
                map((tags: string[]) => {
                    const matchText = this.tagTextEntry.toLowerCase();
                    return tags.filter((tag) => tag.toLowerCase().includes(matchText));
                })
            );
    }

    get isDisabled() {
        return this.control.disabled;
    }

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }

    hasErrors() {
        if (!this.control.errors) {
            return false;
        }
        const errors = Object.keys(this.control.errors);
        return errors.length > 0;
    }

    changeTypeaheadLoading(e: boolean): void {
        this.typeaheadLoading = e;
        this.typeaheadNoResults = false;
    }

    changeTypeaheadNoResults(e: boolean): void {
        this.typeaheadNoResults = e;
    }

    typeaheadOnSelect(e: TypeaheadMatch): void {
        this.addTagValue(e.value);
    }

    addNewTag(e) {
        if (e.key === 'Enter') {
          e.preventDefault();
          if (this.tagTextEntry && this.typeaheadNoResults) {
            this.addTagValue(this.tagTextEntry);
          }
        }
    }

    removeTag(value) {
        this.control.patchValue(removeFromSortedList(this.control.value, value));
        this.syncDisplay();
    }

    private syncDisplay() {
        if (isEqual([...this.originalValue].sort(), [...this.control.value].sort())) {
            this.control.markAsPristine();
            this.control.markAsUntouched();
        } else {
            this.control.markAsDirty();
            this.control.markAsTouched();
        }
    }

    private addTagValue(value) {
        this.control.patchValue(addToSortedList(this.control.value, value.toLowerCase().trim()));
        this.tagTextEntry = '';
        this.typeaheadNoResults = false;
        this.syncDisplay();
    }
}
