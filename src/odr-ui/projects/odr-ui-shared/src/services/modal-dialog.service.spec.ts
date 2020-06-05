// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { async, ComponentFixture, TestBed } from "@angular/core/testing";
import { Component, NgModule, ViewEncapsulation } from "@angular/core";
import { ModalDialogService } from "./modal-dialog.service";
import { ModalModule } from "ngx-bootstrap/modal";
import { BrowserDynamicTestingModule } from '@angular/platform-browser-dynamic/testing';

describe("ModalDialogService", () => {
  let component: TestDialogClientComponent;
  let fixture: ComponentFixture<TestDialogClientComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [TestDialogClientComponent, TestDialogComponent],
      imports: [ModalModule.forRoot()],
      providers: [ModalDialogService],
    })
    .overrideModule(BrowserDynamicTestingModule, {
      set: {
        entryComponents: [ TestDialogComponent ],
      }
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TestDialogClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should contain input values", (done) => {
    expect(component).toBeDefined();
    const hostElement = fixture.nativeElement;
    const openButton: HTMLButtonElement = hostElement.querySelector('button');
    openButton.click();

    const textValues = Array.from(document.querySelectorAll("#test-dialog-text li"));
    expect(textValues.length).toBe(3);
    expect(textValues.map(({textContent}) => textContent)).toEqual([
      'One: 1',
      'Two: 2',
      'Three: 3',
    ]);

    const closeButton: HTMLButtonElement = document.querySelector('#close-dialog-button');
    closeButton.click();
    fixture.whenStable().then(() => {
      fixture.detectChanges();
      done();
    });
  });

  it("should return result", (done) => {
    expect(component).toBeDefined();
    const hostElement = fixture.nativeElement;
    const openButton: HTMLButtonElement = hostElement.querySelector('button');
    openButton.click();

    const closeButton: HTMLButtonElement = document.querySelector('#close-dialog-button');
    closeButton.click();

    fixture.whenStable().then(() => {
      fixture.detectChanges();
      const pElement: HTMLParagraphElement = hostElement.querySelector('p');
      expect(pElement.textContent).toEqual("Status: Was Closed");
      done();
    });
  });
});

interface TestDialogState {
  one: number,
  two: number,
  three: number,
};

@Component({
  selector: `test-dialog`,
  template: `
    <div>
      <p>Test Dialog</p>
      <ul id="test-dialog-text">
        <li>One: {{config.one}}</li>
        <li>Two: {{config.two}}</li>
        <li>Three: {{config.three}}</li>
      </ul>
      <p>
        <button id="close-dialog-button" (click)="closeDialog()">Close Dialog</button>
      </p>
    </div>
  `
})
class TestDialogComponent {
  config: TestDialogState;
  constructor(private modal: ModalDialogService) {}
  closeDialog() {
    this.modal.close({
      text: "Was Closed"
    });
  }
}

@Component({
  selector: `test-dialog-client`,
  template: `
    <div>
      <button (click)="onClick()">Open Dialog</button>
      <p>Status: {{status}}</p>
    </div>
  `,
  styles: [
    `
body.modal-open {
  position: relative;
}

.modal-backdrop {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0,0,0,0.2);
}

.modal {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate3d(-50%, -50%, 0);
  animation: flyinModal 0.2s ease-out;
  will-change: auto;

}

.modal-content {
  padding: 20px;
}

.modal-dialog {
  background: white;
}

    `
  ],
  encapsulation: ViewEncapsulation.None
})
class TestDialogClientComponent {
  status = "Waiting";
  constructor(private modal: ModalDialogService) {}

  onClick() {
    this.modal.create({
      title: "Test Dialog",
      component: TestDialogComponent,
      params: {
        config: {
          one: 1,
          two: 2,
          three: 3
        }
      },
      onClose: (result: { text: string }) => {
        console.log({result});
        this.status = result.text;
      }
    });
  }
}

