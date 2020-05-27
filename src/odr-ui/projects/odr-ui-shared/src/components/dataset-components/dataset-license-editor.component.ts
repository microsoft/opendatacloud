import {
  Component,
  Input,
  OnInit,
  Output,
  EventEmitter,
  OnDestroy,
  OnChanges,
  SimpleChanges
} from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { Observable, Subscription } from "rxjs";
import { License } from "../../models/license.type";
import { LicensesService } from "../../services/licenses.service";
import { matchListId } from "../../utils/list-utils";
import { map, tap } from "rxjs/operators";

export const datasetLicenseFormControl = () => new FormControl();

const OtherLicenseId = LicensesService.otherLicense.id;
const CudaAgreementId = LicensesService.cudaAgreementLicense.id;
const OudaAgreementId = LicensesService.oudaAgreementLicense.id;

@Component({
  selector: "app-dataset-license-editor",
  templateUrl: "./dataset-license-editor.component.html",
  styleUrls: ["./dataset-license-editor.component.scss"]
})
export class DatasetLicenseEditorComponent
  implements OnInit, OnDestroy, OnChanges {
  @Input() control: FormControl;
  @Input() size: number;
  @Input() required: boolean;
  @Input() includeOtherLicense: boolean;
  @Input() updateCounter: number;
  @Output() onEnterLicense: EventEmitter<boolean> = new EventEmitter<boolean>();

  licenses: Observable<License[]>;
  matchLicenseId = matchListId;
  radioSelectControl = new FormControl(OudaAgreementId);
  cudaAgreementId = CudaAgreementId;
  oudaAgreementId = OudaAgreementId;
  showAll = false;

  private subscription: Subscription;
  private lastUpdateCounter: number = 0;

  constructor(private licensesService: LicensesService) {}

  ngOnInit(): void {
    this.licenses = this.licensesService.getLicensesList().pipe(
      map(list => {
        const selectedId = this.control.value
          ? this.control.value.id
          : undefined;
        let foundCurrentId = false;

        const result = list.reduce((list, { id, name }) => {
          const addToList = id !== OtherLicenseId || this.includeOtherLicense;
          foundCurrentId = foundCurrentId || id === selectedId;
          if (addToList) {
            list.push({ id, name });
          }
          return list;
        }, []);

        if (selectedId && !foundCurrentId) {
          const { id, name } = this.control.value;
          result.push({ id, name });
        }

        return result;
      })
    );

    this.subscription = this.radioSelectControl.valueChanges
      .pipe(
        tap(value => {
          switch (value) {
            case OudaAgreementId:
              this.control.setValue(LicensesService.oudaAgreementLicense);
              break;
            case CudaAgreementId:
              this.control.setValue(LicensesService.cudaAgreementLicense);
              break;
          }
        })
      )
      .subscribe();

    if (this.required) {
      this.control.setValidators([Validators.required]);
    }

    this.initializeControl();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.updateCounter) {
      if (this.lastUpdateCounter !== this.updateCounter) {
        this.lastUpdateCounter = this.updateCounter;
        this.initializeControl();
      }
    }
  }

  private initializeControl() {
    let updatedLicense: License | undefined;
    let id: string;

    if (this.control.value && this.control.value.id) {
      id = this.control.value.id;
    } else {
      updatedLicense = LicensesService.oudaAgreementLicense;
      id = updatedLicense.id;
    }

    if (id === OudaAgreementId || id === CudaAgreementId) {
      this.showAll = false;
      this.radioSelectControl.setValue(id);
    } else {
      this.showAll = true;
      if (this.includeOtherLicense && id === OtherLicenseId) {
        updatedLicense = LicensesService.otherLicense;
      }
    }

    if (updatedLicense) {
      this.control.setValue(updatedLicense);
    }
  }

  get isOtherLicenseSelected() {
    return (
      this.includeOtherLicense &&
      this.control.value &&
      this.control.value.id === OtherLicenseId
    );
  }

  get hasLicenseErrors() {
    return Boolean(this.control.errors);
  }

  enterLicense() {
    this.onEnterLicense.emit(true);
  }

  showAllLicenseOptions() {
    this.showAll = true;
  }

  get rootClasses() {
    return [
      this.size ? `col-lg-${this.size}` : "col-lg-12",
      this.hasLicenseErrors ? "has-error" : ""
    ].join(" ");
  }

  get selectedLicenseId() {
    return this.control.value &&
      this.control.value.id &&
      this.control.value.id !== OtherLicenseId
      ? this.control.value.id
      : null;
  }
}
