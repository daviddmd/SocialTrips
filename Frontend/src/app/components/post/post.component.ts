import { Component, OnInit } from '@angular/core';
import {ToastService} from "../../services/toast.service";
import {PostService} from "../../services/post.service";
import {UserService} from "../../services/user.service";
import {AuthenticationService} from "../../services/authentication.service";
import {ActivatedRoute, Router} from "@angular/router";
import {Subscription} from "rxjs";
import {IPost} from "../../shared/interfaces/entities/IPost";

@Component({
  selector: 'app-post',
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css']
})
export class PostComponent implements OnInit {
  private routeSub: Subscription = new Subscription;
  public isAdmin: boolean = false;
  public submittingData : boolean = false;
  public post!: IPost;
  public isLoading: boolean = true;
  //botao para voltar a viagem do post e botao para ver utilizador do post. apenas admin pode aceder ao botao de eliminar directamente
  constructor(
    private toastService: ToastService,
    private postService: PostService,
    private authenticationService : AuthenticationService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.isAdmin = authenticationService.isAdmin();
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      let postId = params["id"];
      this.postService.getPost(postId).subscribe(
        next=>{
          this.post = next as IPost;
          this.isLoading=false;
        },
        error => {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe uma publicação com este ID.");
              break;
            default:
              this.toastService.showError("Erro ao carregar publicação");
          }
          this.router.navigate(["/"]);
        }
      )
    });
  }
  deletePost() : void{
    this.submittingData = true;
    if(confirm("Tem a certeza que pretende remover esta publicação?")){
      this.postService.deletePost(this.post.id).subscribe(
        next=>{
          this.toastService.showSucess("Publicação removida com sucesso");
          this.router.navigate(['/trip', this.post.trip.id]);
        },
        error => {
          this.toastService.showError("Erro ao remover publicação");
        }
      ).add(
        ()=>{
          this.submittingData=false;
        }
      )
    }
  }
}
