import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { StoreModule } from '@ngrx/store';
import { HttpModule } from '@angular/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { SharedModule } from './shared/shared.module';
import { CoreModule } from './core/core.module';
import { HomeModule } from './home/home.module';

import { ApiTranslationLoader } from './shared/services/api-translation-loader.service';

import { routing } from './app.routes';
import { AppService } from './app.service';
import { appReducer } from './app-store';
import { AppComponent } from './app.component';
import { ErrorHandler } from "./apiDefinitions";
// import { FileUploadModule } from "ng2-file-upload";
// import { FileSelectDirective, FileUploader } from "ng2-file-upload";

@NgModule({
    declarations: [AppComponent /*, FileSelectDirective, FileUploader*/],
    imports: [
        BrowserAnimationsModule,
        BrowserModule,
        routing,
        // FormsModule,
        HttpModule,
        // Only module that app module loads
        CoreModule.forRoot(),
        SharedModule.forRoot(),
        HomeModule,
        StoreModule.provideStore(appReducer),
        StoreDevtoolsModule.instrumentOnlyWithExtension(),
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: ApiTranslationLoader } })
        // ,FileUploadModule
    ],
    providers: [
        AppService, ErrorHandler
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
