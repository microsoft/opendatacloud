import { Injectable } from "@angular/core";
import { BsModalService } from "ngx-bootstrap/modal";
import { take } from "rxjs/operators";

const MODAL_OPEN_CLASS_NAME = "modal-open";

export interface ModalConfiguration {
  title: string;
  component: any;
  params?: any;
  onClose?: (result: any) => void;
}

@Injectable()
export class ModalDialogService {
  private currentModalResult: any;

  constructor(private modalService: BsModalService) {}

  public create(configuration: ModalConfiguration) {
    const { component, params, onClose } = configuration;
    this.modalService.show(component, {
      animated: true,
      initialState: params,
      keyboard: true,
      show: true
    });

    this.currentModalResult = undefined;
    if (onClose) {
      this.modalService.onHidden.pipe(take(1)).subscribe({
        next: () => {
          onClose(this.popLastResult());
        }
      });
    }
  }

  public close(result?: any): void {
    const count = this.modalService.getModalsCount();
    if (count > 0) {
      this.currentModalResult = result;
      this.modalService.hide(count);
      if (count === 1) {
        // Work-around for bug in ngx-bootstrap modal service
        if (document && document.body) {
          document.body.classList.remove(MODAL_OPEN_CLASS_NAME);
        }
      }
    }
  }

  private popLastResult() {
    const result = this.currentModalResult;
    this.currentModalResult = undefined;
    return result;
  }
}
