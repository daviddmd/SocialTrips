import {Injectable, TemplateRef} from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts: any[] = [];
  constructor() { }
  show(textOrTpl: string | TemplateRef<any>, options: any = {}) {
    this.toasts.push({ textOrTpl, ...options });
  }
  showSucess(text: string){
    this.show(text,{ classname: 'bg-success text-light'})
  }
  showError(text: string){
    this.show(text,{ classname: 'bg-danger text-light'})
  }
  remove(toast: any) {
    this.toasts = this.toasts.filter(t => t != toast);
  }
}
