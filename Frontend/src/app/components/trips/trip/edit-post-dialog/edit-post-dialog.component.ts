import {Component, Inject, OnInit} from '@angular/core';
import {Form, FormBuilder, FormGroup, Validators} from "@angular/forms";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {TripComponent} from "../trip.component";
import {ToastService} from "../../../../services/toast.service";
import {ITrip} from "../../../../shared/interfaces/entities/ITrip";
import {IPost} from "../../../../shared/interfaces/entities/IPost";
import {IPostEditDialogData} from "../../../../shared/interfaces/IPostEditDialogData";
import {PostService} from "../../../../services/post.service";
import {IAttachment} from "../../../../shared/interfaces/entities/IAttachment";
import {IPostEditDialogReturnData} from "../../../../shared/interfaces/IPostEditDialogReturnData";

@Component({
  selector: 'app-edit-post-dialog',
  templateUrl: './edit-post-dialog.component.html',
  styleUrls: ['./edit-post-dialog.component.css']
})
export class EditPostDialogComponent implements OnInit {
  public postFileToUpload: File | null = null;
  public submittingData: boolean = false;
  public postAttachmentForm: FormGroup;
  public postEditForm: FormGroup;
  public minimumDate: Date;
  public maximumDate: Date;
  public post: IPost;
  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<TripComponent>,
    private toastService: ToastService,
    private postService: PostService,
    @Inject(MAT_DIALOG_DATA) dialogData: IPostEditDialogData
  ) {
    this.post = dialogData.post;
    this.minimumDate = new Date(dialogData.trip.beginningDate);
    this.maximumDate = new Date(dialogData.trip.endingDate);
    this.postEditForm = this.formBuilder.group({
      description: [dialogData.post.description, Validators.required],
      date: [dialogData.post.date, Validators.required],
      isHidden: [dialogData.post.isHidden]
    });
    this.postAttachmentForm = this.formBuilder.group({
      attachment: ["", Validators.required]
    })
  }

  get attachment() {
    return this.postAttachmentForm.get("attachment")!;
  }

  get description() {
    return this.postEditForm.get("description")!;
  }

  get date() {
    return this.postEditForm.get("date")!;
  }

  ngOnInit(): void {
  }

  //este save é unicamente para "guardar" edições como descrição, data, ou se está escondido.
  //para adicionar posts, temos um formulário de ficheiro diferente que interage directamente
  save() {
    if (this.postEditForm.valid) {
      this.dialogRef.close(this.postEditForm.value);
    } else {
      this.toastService.showError("Corriga os erros do formulário de criar publicação");
    }
  }

  close() {
    this.dialogRef.close();
  }

  addImageToPost() {
    if (this.postFileToUpload == null) {
      this.toastService.showError("Insira o anexo na publicação que pretende adicionar anexo.");
      return;
    }
    this.submittingData = true;
    this.postService.addAttachmentPost(this.post.id, this.postFileToUpload).subscribe(
      next => {
        this.post.attachments = next.attachments;
        this.toastService.showSucess("Anexo adicionado à publicação com sucesso");
      },
      error => {
        this.toastService.showError("Erro ao actualizar publicação");
      }
    ).add(() => {
      this.submittingData = false;
    })
    this.postFileToUpload=null;
  }

  removeAttachmentFromPost(attachment : IAttachment) {
    this.submittingData = true;
    this.postService.remoteAttachmentPost(attachment.id).subscribe(
      next => {
        this.post.attachments = this.post.attachments.filter(a=>a.id!=attachment.id);
        this.toastService.showSucess("Anexo removido da publicação com sucesso");
      },
      error => {
        this.toastService.showError("Erro ao remover anexo");
      }
    ).add(() => {
      this.submittingData = false;
    })
  }

  onAttachmentChange(event: any) {
    this.postFileToUpload = event.target.files.item(0);
  }

}
