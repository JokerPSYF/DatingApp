import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModaRef?: BsModalRef<ConfirmDialogComponent>;

  constructor(private modalService: BsModalService) { }

  confirm(
    title = 'Confirmation',
    message = 'Are you sure you want to do this?',
    btnOkText = 'Ok',
    btnCancelText = 'Cancel'
  ): Observable<boolean> {
    const config = {
      initialState : {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModaRef = this.modalService.show(ConfirmDialogComponent, config);
    return this.bsModaRef.onHidden!.pipe(
      map(() => {
        return this.bsModaRef!.content!.result
      })
    )
  }
}